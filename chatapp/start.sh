#!/bin/bash
set -e

echo "🚀 Starting ChatApp..."
echo ""

# Check prerequisites
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK not found. Install from https://dotnet.microsoft.com/download"
    exit 1
fi

if ! command -v node &> /dev/null; then
    echo "❌ Node.js not found. Install from https://nodejs.org"
    exit 1
fi

# Install frontend deps if needed
if [ ! -d "client/node_modules" ]; then
    echo "📦 Installing frontend dependencies..."
    cd client && npm install && cd ..
fi

echo "✓ Starting backend on http://localhost:5000"
cd ChatApp.Api && dotnet run &
BACKEND_PID=$!

echo "✓ Starting frontend on http://localhost:5173"
cd ../client && npm run dev &
FRONTEND_PID=$!

echo ""
echo "🎉 ChatApp is running!"
echo "   Open: http://localhost:5173"
echo ""
echo "Press Ctrl+C to stop both servers"

# Wait for both and cleanup
trap "kill $BACKEND_PID $FRONTEND_PID 2>/dev/null; echo 'Stopped.'" SIGINT SIGTERM
wait
