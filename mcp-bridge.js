#!/usr/bin/env node

/**
 * MCP stdio-to-HTTP bridge for Claude Desktop
 * Convierte las llamadas stdio de Claude Desktop a HTTP calls al Azure Functions MCP server
 */

const http = require('http');
const https = require('https');
const readline = require('readline');
const fs = require('fs');
const path = require('path');

// Cargar configuración desde archivo
const configPath = path.join(__dirname, 'mcp-bridge-config.json');
let config = { mcpServer: { protocol: 'http', hostname: 'localhost', port: 7073, path: '/api/mcp' } };

if (fs.existsSync(configPath)) {
  try {
    config = JSON.parse(fs.readFileSync(configPath, 'utf8'));
    console.error(`[BRIDGE] Config loaded from ${configPath}`);
  } catch (e) {
    console.error(`[BRIDGE] Error loading config, using defaults: ${e.message}`);
  }
}

const MCP_SERVER_URL = `${config.mcpServer.protocol}://${config.mcpServer.hostname}:${config.mcpServer.port}${config.mcpServer.path}`;
console.error(`[BRIDGE] Connecting to: ${MCP_SERVER_URL}`);

// Mantener el proceso vivo
process.stdin.resume();

// Crear interfaz para leer stdin línea por línea
// IMPORTANTE: No usar output: process.stdout para evitar contaminar las respuestas JSON
const rl = readline.createInterface({
  input: process.stdin,
  terminal: false
});

// Función para enviar request al servidor MCP HTTP
function sendToMcpServer(request) {
  return new Promise((resolve, reject) => {
    const postData = JSON.stringify(request);
    
    const httpModule = config.mcpServer.protocol === 'https' ? https : http;
    
    const options = {
      hostname: config.mcpServer.hostname,
      port: config.mcpServer.port,
      path: config.mcpServer.path,
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Content-Length': Buffer.byteLength(postData)
      },
      // Desactivar verificación SSL para desarrollo
      rejectUnauthorized: false
    };

    const req = httpModule.request(options, (res) => {
      let data = '';

      res.on('data', (chunk) => {
        data += chunk;
      });

      res.on('end', () => {
        // Handle 204 No Content (notifications don't need response)
        if (res.statusCode === 204) {
          resolve(null); // No response for notifications
          return;
        }
        
        try {
          const response = JSON.parse(data);
          resolve(response);
        } catch (e) {
          reject(new Error('Invalid JSON response from MCP server'));
        }
      });
    });

    req.on('error', (e) => {
      console.error(`[BRIDGE] HTTP request error: ${e.message}`);
      reject(e);
    });

    req.write(postData);
    req.end();
  });
}

// Validar y limpiar la respuesta para que cumpla con el schema MCP
function validateAndCleanResponse(response, requestId) {
  // Crear respuesta limpia desde cero
  const cleanResponse = {
    jsonrpc: "2.0",
    id: (response.id !== null && response.id !== undefined) ? response.id : (requestId || 1)
  };

  // Si tiene result, copiarlo (sin error)
  if (response.result !== null && response.result !== undefined) {
    cleanResponse.result = response.result;
    
    // Validar estructura de tools/list
    if (cleanResponse.result.tools && Array.isArray(cleanResponse.result.tools)) {
      cleanResponse.result.tools = cleanResponse.result.tools.map(tool => ({
        name: String(tool.name || ''),
        description: String(tool.description || ''),
        inputSchema: tool.inputSchema || {
          type: "object",
          properties: {},
          required: []
        }
      }));
    }

    // Validar estructura de tools/call
    if (cleanResponse.result.content && Array.isArray(cleanResponse.result.content)) {
      cleanResponse.result.content = cleanResponse.result.content.map(item => ({
        type: String(item.type || 'text'),
        text: String(item.text || '')
      }));
    }
  }
  // Si tiene error, copiarlo (sin result)
  else if (response.error) {
    cleanResponse.error = {
      code: Number(response.error.code || -32603),
      message: String(response.error.message || 'Unknown error')
    };
    if (response.error.data !== undefined && response.error.data !== null) {
      cleanResponse.error.data = response.error.data;
    }
  }

  return cleanResponse;
}

// Procesar cada línea de stdin
rl.on('line', async (line) => {
  try {
    const request = JSON.parse(line);
    const requestId = request.id || 1;
    
    console.error(`[BRIDGE] Received request - method: ${request.method}, id: ${requestId}`);
    
    const response = await sendToMcpServer(request);
    
    // Notifications return null (204 No Content), skip response
    if (response === null) {
      console.error(`[BRIDGE] Notification processed, no response needed`);
      return;
    }
    
    console.error(`[BRIDGE] Received response from server`);
    
    const cleanedResponse = validateAndCleanResponse(response, requestId);
    console.error(`[BRIDGE] Sending cleaned response to Claude`);
    
    // Enviar respuesta a stdout (Claude Desktop lo leerá)
    process.stdout.write(JSON.stringify(cleanedResponse) + '\n');
  } catch (error) {
    console.error(`[BRIDGE] Error processing request: ${error.message}`);
    console.error(`[BRIDGE] Stack: ${error.stack}`);
    const errorResponse = {
      jsonrpc: '2.0',
      id: 1,
      error: {
        code: -32603,
        message: String(error.message || 'Internal error')
      }
    };
    process.stdout.write(JSON.stringify(errorResponse) + '\n');
  }
});

rl.on('close', () => {
  console.error('[BRIDGE] stdin closed, exiting');
  process.exit(0);
});

// Manejar señales de terminación
process.on('SIGTERM', () => {
  console.error('[BRIDGE] Received SIGTERM, exiting');
  process.exit(0);
});

process.on('SIGINT', () => {
  console.error('[BRIDGE] Received SIGINT, exiting');
  process.exit(0);
});

// Manejar errores de proceso
process.on('uncaughtException', (error) => {
  console.error('[BRIDGE] Uncaught exception:', error);
  process.exit(1);
});

// Log de inicio
console.error('[BRIDGE] MCP Bridge started, waiting for messages...');
