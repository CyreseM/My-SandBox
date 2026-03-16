import * as signalR from '@microsoft/signalr';

let connection = null;

export function getConnection() {
  if (!connection) {
    connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/chat', { withCredentials: true })
      .withAutomaticReconnect([0, 1000, 3000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Warning)
      .build();
  }
  return connection;
}

export async function startConnection() {
  const conn = getConnection();
  if (conn.state === signalR.HubConnectionState.Disconnected) {
    await conn.start();
  }
  return conn;
}

export async function stopConnection() {
  if (connection?.state !== signalR.HubConnectionState.Disconnected) {
    await connection?.stop();
  }
}
