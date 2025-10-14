![](https://img.shields.io/badge/For%20Final%20Review-orange)
![](https://img.shields.io/badge/Collect%20Feedback-orange)

# Challenge 01 - Accelerate your developer productivity with MCP servers in Visual Studio Code

 [< Previous Challenge](./Challenge-00.md) - **[Home](../README.md)** - [Next Challenge >](./Challenge-02-csharp.md)
 
## Introduction

In this challenge, you'll set up GitHub Copilot in Visual Studio Code and explore its powerful features including Ask, Edit, and Agent modes. You'll also learn how to leverage Model Context Protocol (MCP) servers to enhance your development workflow with AI-powered assistance for building intelligent applications.

## Concepts

### GitHub Copilot

GitHub Copilot is an AI-powered code completion tool that acts as your programming pair partner. It helps you write code faster and with less effort by providing context-aware code suggestions, generating boilerplate code, and even creating entire functions or classes based on your comments or existing code.

#### Key Features

- Code completions: Context-aware inline suggestions and next-edit predictions.
- Natural language chat: Ask, explain, and apply multi-file changes with prompts.
- Autonomous coding (agent mode): Plan and execute multi-step tasks with tools and terminal access.
- Smart actions: AI-enhanced commit messages, PR descriptions, renames, fixes, and semantic search.

#### GitHub Copilot Modes

**1. Code completions**
Copilot provides inline code suggestions as you type, ranging from single line completions to entire function implementations. With next edit suggestions, it predicts the next logical code change based on your current context.

**2. Chat Mode (Ask)**
Chat in Visual Studio Code lets you use natural language to interact with large language models (LLMs) to get help with your code. Ask mode for chat is optimized for asking questions about your codebase, coding, and general technology concepts. Ask mode is particularly useful for getting a better understanding of your codebase, brainstorming ideas, and getting help with coding tasks.

**3. Edit Mode**
Use edit mode when you want more granular control over the edits that Copilot proposes. In edit mode, you choose which files Copilot can make changes to, provide context to Copilot with each iteration, and decide whether or not to accept the suggested edits after each turn.

Edit mode is best suited to use cases where:

- You want to make a quick, specific update to a defined set of files.
- You want full control over the number of LLM requests Copilot uses.

**4. Agent Mode**
Use agent mode when you have a specific task in mind and want to enable Copilot to autonomously edit your code. In agent mode, Copilot determines which files to make changes to, offers code changes and terminal commands to complete the task, and iterates to remediate issues until the original task is complete.

Agent mode is best suited to use cases where:

- Your task is complex, and involves multiple steps, iterations, and error handling.
- You want Copilot to determine the necessary steps to take to complete the task.
- The task requires Copilot to integrate with external applications, such as an MCP server.

#### Model Context Protocol (MCP)

MCP is a protocol that enables AI models to access external data sources and tools securely. In VS Code, MCP servers provide enhanced context to Copilot, making suggestions more accurate and relevant to your specific environment.

**Benefits:**
- Enhanced context awareness
- Integration with external tools and services
- Secure access to private repositories and resources
- Improved code suggestions based on your project structure

## Description

Your challenge is to set up and get familiar with GitHub Copilot in VS Code, then enhance it with MCP servers to accelerate your AI application development workflow.

### Task 1: Setup and Configure GitHub Copilot
Setup GitHub Copilot in Visual Studio Code, and confirm the installation by generating an inline code suggestion.

### Task 2: Explore Copilot Interaction Modes

Demonstrate Copilot's different modes by completing a simple coding task: create a method to add two numbers in your preferred programming language. Try the following in each mode:

#### Inline Suggestions
- Start typing a function definition (e.g., `decimal AddNumbers(string a, string b)`) and observe Copilot's inline suggestion to complete the function.

#### Chat Mode (Ask)
- Open Copilot Chat and ask:  
  *"Can you show me how to write a function in C# that adds two numbers?"*

#### Edit Mode
- Write a basic function with a placeholder body, select it, and prompt:  
  *"Implement this function to return the sum of the two arguments."*

#### Agent Mode
- In Copilot Chat, use an agent command such as:  
  *"@workspace Create a function in JavaScript that adds two numbers and write a test for it."*

Try these examples in your editor to see how Copilot assists you differently in each mode.

### Task 3: Configure MCP Servers

Integrate an MCP server or tool with Microsoft Docs and Playwright; demonstrate it by retrieving docs content or running a Playwright action via chat.

#### How to Use MCP Servers in Visual Studio Code

For detailed instructions on enhancing GitHub Copilot with MCP servers in Visual Studio Code, refer to the official documentation: [Customize Copilot with MCP Servers in VS Code](https://code.visualstudio.com/docs/copilot/customization/mcp-servers).

#### MS Docs

MS Docs MCP (Model Context Protocol) is a framework that allows developers to access and utilize Microsoft documentation and learning resources directly within their development environment. By integrating MS Docs MCP into tools like VS Code, developers can quickly retrieve relevant documentation, examples, and best practices while working on their projects.

Use an MCP integration in VS Code to connect to Microsoft Docs/Learn. In Copilot Chat (Agent mode), ask via the MS Docs MCP server: "What are the key benefits of GitHub Copilot?" Then post back with the following format:

- The canonical Microsoft Docs link used
- A 2–3 sentence summary with one short quoted line
- Bullet points of key benefits

#### Browser Automation with Playwright

Playwright is a tool that lets you automate web browsers with code. You can use it to open pages, click buttons, fill forms, and get information from websites easily.

Playwright can leverage MCP to access documentation and examples related to browser automation, making it easier for developers to implement and troubleshoot their automation scripts. By integrating with MCP, Playwright can provide context-aware suggestions and resources directly within the development environment.

Using Playwright, interact in the Copilot Chat window to automate Google Maps and obtain driving directions from "Piccadilly Circus, London" to "University of Surrey, Guildford". Extract and print the main route’s travel duration and distance to the console.

## Success Criteria

- ✅ GitHub Copilot is configured on VS Code
- ✅ Successfully demonstrate inline code suggestions
- ✅ Successfully use Edit mode to refactor existing code with specific instructions
- ✅ Interact with @workspace, @vscode, and @terminal agents effectively
- ✅ Demonstrate understanding of when to use each Copilot mode for different tasks
- ✅ Use an MCP integration to fetch a Microsoft Docs article
- ✅ Use Playwright to automate Google Maps driving directions

## Learning Resources
- [Set up GitHub Copilot in VS Code](https://code.visualstudio.com/docs/copilot/setup)
- [GitHub Copilot in VS Code](https://code.visualstudio.com/docs/editor/artificial-intelligence)
- [Use MCP servers in VS Code](https://code.visualstudio.com/docs/copilot/customization/mcp-servers)
- [GitHub Copilot Chat Documentation](https://docs.github.com/en/copilot/using-github-copilot/asking-github-copilot-questions-in-your-ide)
- [Model Context Protocol (MCP) Overview](https://modelcontextprotocol.io/)
- [VS Code MCP (Model Context Protocol)](https://code.visualstudio.com/mcp)
- [GitHub Copilot Best Practices](https://docs.github.com/en/copilot/using-github-copilot/best-practices-for-using-github-copilot)
- [Playwright Docs](https://playwright.dev/docs/intro)
