# WinForms MCP Server - Configuration Guide

## Overview

The **fnWindowsMCP** (WinForms MCP Server) is a Model Context Protocol (MCP) server that enables automation of WinForms applications. It provides tools for UI automation, process management, and testing through a JSON-RPC interface over stdio.

### Features

This MCP server provides the following capabilities:

- **Element Discovery**: Find UI elements by AutomationId, Name, ClassName, or ControlType
- **UI Interaction**: Click, type text, set values, drag-and-drop
- **Process Management**: Launch applications, attach to processes, close apps
- **Validation**: Take screenshots, check element existence, wait for elements
- **Keyboard Control**: Send arbitrary key sequences

## Prerequisites

### System Requirements

- **Operating System**: Windows (UI Automation is Windows-specific)
- **.NET Runtime**: .NET 8.0 or later
- **Visual Studio Code**: Latest version
- **Cline Extension**: Install from VS Code marketplace

### Build Requirements

To build the server from source:

```powershell
# Navigate to project directory
cd F:\projects\winforms-mcp

# Restore dependencies
dotnet restore

# Build the project
dotnet build src\Rhombus.WinFormsMcp.Server\Rhombus.WinFormsMcp.Server.csproj -c Release

# Publish as self-contained executable (recommended for MCP)
dotnet publish src\Rhombus.WinFormsMcp.Server\Rhombus.WinFormsMcp.Server.csproj -c Release -r win-x64 --self-contained
```

The compiled executable will be located at:
```
src\Rhombus.WinFormsMcp.Server\bin\Release\net8.0\win-x64\publish\Rhombus.WinFormsMcp.Server.exe
```

## Configuration for VS Code and Cline

### Method 1: Using Cline MCP Settings (Recommended)

Cline stores MCP server configurations in a JSON file. The location depends on your system:

**Windows**: `%APPDATA%\Code\User\globalStorage\saoudrizwan.claude-dev\settings\cline_mcp_settings.json`

**macOS**: `~/Library/Application Support/Code/User/globalStorage/saoudrizwan.claude-dev/settings/cline_mcp_settings.json`

**Linux**: `~/.config/Code/User/globalStorage/saoudrizwan.claude-dev/settings/cline_mcp_settings.json`

#### Step 1: Locate or Create the Configuration File

1. Open VS Code
2. Install the **Cline** extension if not already installed
3. Open the Command Palette (`Ctrl+Shift+P` or `Cmd+Shift+P`)
4. Search for "Cline: Open MCP Settings"
5. This will create/open the `cline_mcp_settings.json` file

#### Step 2: Add WinForms MCP Server Configuration

Add the following configuration to the `mcpServers` section:

```json
{
  "mcpServers": {
    "winforms-automation": {
      "command": "F:\\projects\\winforms-mcp\\src\\Rhombus.WinFormsMcp.Server\\bin\\Release\\net8.0\\win-x64\\publish\\Rhombus.WinFormsMcp.Server.exe",
      "args": [],
      "env": {
        "DOTNET_ENVIRONMENT": "Production"
      }
    }
  }
}
```

#### Step 3: Alternative - Using dotnet run (Development)

For development purposes, you can also configure it to run via `dotnet`:

```json
{
  "mcpServers": {
    "winforms-automation": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "F:\\projects\\winforms-mcp\\src\\Rhombus.WinFormsMcp.Server\\Rhombus.WinFormsMcp.Server.csproj"
      ],
      "env": {
        "DOTNET_ENVIRONMENT": "Development"
      }
    }
  }
}
```

### Method 2: Using VS Code Settings.json

You can also configure MCP servers through VS Code's `settings.json`:

1. Open VS Code Settings (`Ctrl+,` or `Cmd+,`)
2. Click the "Open Settings (JSON)" icon in the top right
3. Add the following configuration:

```json
{
  "cline.mcpServers": {
    "winforms-automation": {
      "command": "F:\\projects\\winforms-mcp\\src\\Rhombus.WinFormsMcp.Server\\bin\\Release\\net8.0\\win-x64\\publish\\Rhombus.WinFormsMcp.Server.exe",
      "args": [],
      "env": {}
    }
  }
}
```

### Method 3: Workspace-Specific Configuration

For project-specific MCP server configuration:

1. Create or open `.vscode/settings.json` in your workspace
2. Add the MCP server configuration:

```json
{
  "cline.mcpServers": {
    "winforms-automation": {
      "command": "${workspaceFolder}\\src\\Rhombus.WinFormsMcp.Server\\bin\\Release\\net8.0\\win-x64\\publish\\Rhombus.WinFormsMcp.Server.exe",
      "args": [],
      "env": {
        "DOTNET_ENVIRONMENT": "Production"
      }
    }
  }
}
```

## Complete Configuration Example

Here's a complete `cline_mcp_settings.json` example with multiple MCP servers:

```json
{
  "mcpServers": {
    "winforms-automation": {
      "command": "F:\\projects\\winforms-mcp\\src\\Rhombus.WinFormsMcp.Server\\bin\\Release\\net8.0\\win-x64\\publish\\Rhombus.WinFormsMcp.Server.exe",
      "args": [],
      "env": {
        "DOTNET_ENVIRONMENT": "Production",
        "LOG_LEVEL": "Information"
      },
      "disabled": false
    }
  }
}
```

## Verifying the Configuration

### Step 1: Restart VS Code

After adding the configuration, restart VS Code to ensure Cline loads the new MCP server.

### Step 2: Check Cline Output

1. Open the Output panel in VS Code (`View > Output` or `Ctrl+Shift+U`)
2. Select "Cline" from the dropdown
3. Look for initialization messages from the WinForms MCP server

### Step 3: Test the Server

Open Cline chat and try a command like:

```
Can you list the available tools from the winforms-automation MCP server?
```

Cline should respond with the available tools:
- find_element
- click_element
- type_text
- set_value
- get_property
- launch_app
- attach_to_process
- close_app
- take_screenshot
- element_exists
- wait_for_element
- drag_drop
- send_keys
- raise_event (not yet implemented)
- listen_for_event (not yet implemented)

## Using the MCP Server with Cline

Once configured, you can interact with WinForms applications through Cline. Here are some example prompts:

### Example 1: Launch and Automate Calculator

```
Using the winforms-automation server:
1. Launch the Windows Calculator app
2. Find the button with AutomationId "num7Button"
3. Click it
4. Take a screenshot and save it to C:\temp\calculator.png
```

### Example 2: Automate a Custom WinForms App

```
Using winforms-automation:
1. Launch my app at F:\MyApp\bin\Debug\MyApp.exe
2. Wait for an element with AutomationId "loginButton" to appear
3. Find the username textbox (AutomationId: "usernameTextBox")
4. Type "testuser" into it
5. Find the password textbox and type "password123"
6. Click the login button
```

### Example 3: Testing UI Elements

```
Using winforms-automation, check if an element with AutomationId "submitButton" exists in my running application, and if it does, get its "IsEnabled" property.
```

## Troubleshooting

### Server Not Starting

**Problem**: Cline shows "Failed to start MCP server"

**Solutions**:
1. Verify the executable path is correct
2. Ensure .NET 8.0 runtime is installed
3. Check Windows permissions for the executable
4. Review the Cline output panel for error messages

### Server Crashes on Startup

**Problem**: Server starts but immediately crashes

**Solutions**:
1. Run the executable manually from PowerShell to see error messages:
   ```powershell
   F:\projects\winforms-mcp\src\Rhombus.WinFormsMcp.Server\bin\Release\net8.0\win-x64\publish\Rhombus.WinFormsMcp.Server.exe
   ```
2. Check for missing dependencies (FlaUI libraries)
3. Ensure UI Automation is enabled on Windows

### Tools Not Available

**Problem**: Cline doesn't show the MCP server tools

**Solutions**:
1. Check the JSON configuration syntax is valid
2. Verify the server name matches in configuration
3. Restart VS Code completely
4. Check Cline extension is up to date

### UI Automation Failures

**Problem**: Elements cannot be found or interacted with

**Solutions**:
1. Ensure the target application is running
2. Verify AutomationIds are correct (use Inspect.exe from Windows SDK)
3. Check if UI Automation is enabled in the target application
4. Ensure the server has necessary Windows permissions

## Advanced Configuration

### Environment Variables

You can pass additional environment variables to the MCP server:

```json
{
  "mcpServers": {
    "winforms-automation": {
      "command": "path\\to\\Rhombus.WinFormsMcp.Server.exe",
      "args": [],
      "env": {
        "DOTNET_ENVIRONMENT": "Production",
        "LOG_LEVEL": "Debug",
        "AUTOMATION_TIMEOUT": "30000"
      }
    }
  }
}
```

### Multiple Instances

You can configure multiple instances for different purposes:

```json
{
  "mcpServers": {
    "winforms-dev": {
      "command": "dotnet",
      "args": ["run", "--project", "F:\\projects\\winforms-mcp\\src\\Rhombus.WinFormsMcp.Server\\Rhombus.WinFormsMcp.Server.csproj"],
      "env": { "DOTNET_ENVIRONMENT": "Development" }
    },
    "winforms-prod": {
      "command": "F:\\projects\\winforms-mcp\\src\\Rhombus.WinFormsMcp.Server\\bin\\Release\\net8.0\\win-x64\\publish\\Rhombus.WinFormsMcp.Server.exe",
      "env": { "DOTNET_ENVIRONMENT": "Production" }
    }
  }
}
```

### Disabling the Server

To temporarily disable the server without removing the configuration:

```json
{
  "mcpServers": {
    "winforms-automation": {
      "command": "path\\to\\Rhombus.WinFormsMcp.Server.exe",
      "disabled": true
    }
  }
}
```

## Protocol Details

### JSON-RPC Communication

The server communicates using JSON-RPC 2.0 over stdio. It supports the following MCP methods:

- `initialize`: Initialize the server and return capabilities
- `tools/list`: Get list of available tools
- `tools/call`: Execute a specific tool

### Tool Call Example

Here's how Cline calls a tool internally:

```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "tools/call",
  "params": {
    "name": "find_element",
    "arguments": {
      "automationId": "num7Button"
    }
  }
}
```

Response:

```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "result": {
    "content": [
      {
        "type": "text",
        "text": "{\"success\": true, \"elementId\": \"elem_1\", \"name\": \"Seven\", \"automationId\": \"num7Button\", \"controlType\": \"Button\"}"
      }
    ]
  }
}
```

## Security Considerations

1. **Process Permissions**: The MCP server runs with the same permissions as VS Code
2. **Application Access**: Can interact with any UI application accessible to the current user
3. **File System**: Can launch executables and save screenshots to accessible locations
4. **Trusted Code**: Only run this MCP server in trusted environments

## Additional Resources

- [Model Context Protocol Specification](https://modelcontextprotocol.io/)
- [FlaUI Documentation](https://github.com/FlaUI/FlaUI)
- [Cline Extension](https://marketplace.visualstudio.com/items?itemName=saoudrizwan.claude-dev)
- [Windows UI Automation](https://docs.microsoft.com/en-us/windows/win32/winauto/entry-uiauto-win32)

## Support and Contributing

For issues, feature requests, or contributions:
- Repository: https://github.com/osubeaver808/winforms-mcp
- Issues: https://github.com/osubeaver808/winforms-mcp/issues

---

**Last Updated**: January 2025  
**Version**: 1.0.0  
**Compatible with**: Cline 2.0+, VS Code 1.85+
