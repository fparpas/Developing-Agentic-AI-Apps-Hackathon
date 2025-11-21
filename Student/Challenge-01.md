# Challenge 01 - Accelerate Developer Productivity with MCP Servers in Visual Studio Code

 [< Previous Challenge](./Challenge-00.md) - **[Home](../README.md)** - [Next Challenge >](./Challenge-02-csharp.md)
 
## Introduction

In this challenge, you'll set up GitHub Copilot in Visual Studio Code and explore its powerful features, including Ask, Edit, Plan and Agent modes. You'll also learn how to leverage Model Context Protocol (MCP) servers to enhance your development workflow with AI‑powered assistance for building intelligent applications.

## Concepts

### GitHub Copilot

GitHub Copilot is an AI‑powered coding assistant that acts as your pair programming partner. It helps you write code faster and with less effort by providing context‑aware suggestions, generating boilerplate code, and even creating entire functions or classes based on comments or existing code.

#### Key Features

- Code completions: Context‑aware inline suggestions and next‑edit predictions
- Natural language chat: Ask, explain, and apply multi‑file changes with prompts
- Autonomous coding (Agent and Plan Mode): Plan and execute multi‑step tasks with tools and terminal access
- Smart actions: AI‑enhanced commit messages, PR descriptions, renames, fixes, and semantic search

#### GitHub Copilot Modes

**1. Code completions**
Copilot provides inline suggestions as you type, ranging from single‑line completions to entire function implementations. With next‑edit suggestions, it predicts the next logical code change based on your current context.

**2. Chat Mode (Ask)**
Chat in Visual Studio Code lets you use natural language to interact with large language models (LLMs) for help with your code. Ask Mode is optimized for questions about your codebase, coding techniques, and general technology concepts. It is particularly useful for understanding unfamiliar code, brainstorming ideas, and getting guidance on implementation tasks.

**3. Edit Mode**
Use Edit Mode when you want granular control over the changes Copilot proposes. You choose which files Copilot may modify, provide iterative context, and decide whether to accept suggested edits after each turn.

Edit Mode is best suited for cases where:

- You want to make a quick, specific update to a defined set of files
- You want full control over the number of LLM requests Copilot uses

**4. Agent Mode**
Use Agent Mode when you have a specific task and want Copilot to autonomously edit your code. Copilot determines which files to change, proposes code edits and terminal commands, and iterates to remediate issues until the task is complete.

Agent Mode is best suited for cases where:

- The task is complex and involves multiple steps, iterations, or error handling
- You want Copilot to determine the necessary steps to complete the task
- The task requires integration with external applications (e.g., an MCP server)

**5. Plan Mode**
Use Plan Mode when you want Copilot to generate a structured, multi‑step plan before any code changes are made. Copilot analyzes your high‑level goal, enumerates actionable steps (e.g., create files, refactor code, add tests), and waits for your approval before executing edits or running commands.

Plan Mode is best suited for cases where:

- You need clarity and scope definition before modifying the codebase
- You want to review, reorder, or prune proposed steps prior to execution
- The task spans multiple domains (code, tests, configuration) and benefits from a phased approach

Example prompt:

> "Plan an update to migrate our data access layer to use async methods, add logging, and include unit tests. Do not modify code yet—just produce the plan."

After reviewing the plan, you can instruct Copilot to proceed (optionally step‑by‑step) or request revisions for greater precision.

#### Model Context Protocol (MCP)

MCP is a protocol that enables AI models to access external data sources and tools securely. In VS Code, MCP servers provide enhanced context to Copilot, making suggestions more accurate and relevant to your development environment.

**Benefits:**
- Enhanced context awareness
- Integration with external tools and services
- Secure access to private repositories and resources
- Improved suggestions based on project structure

## Description

Your challenge is to set up and become familiar with GitHub Copilot in VS Code, then enhance it with MCP servers to accelerate your AI application development workflow.

### Task 1: Set Up and Configure GitHub Copilot
Set up GitHub Copilot in Visual Studio Code, and confirm the installation by generating an inline code suggestion.

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

#### Plan Mode

- In Copilot Chat, request a plan first:  
  *"@workspace Plan creating a function in Python that adds two numbers, then implement it and write a unit test. Produce only the plan."*  
- Review, reorder, or refine the proposed steps (e.g., create file, implement function, add test). Approve the plan before allowing Copilot to execute changes.

Try these examples in your editor to see how Copilot assists you differently in each mode.

### Task 3: Configure MCP Servers

Integrate MCP servers for Microsoft Docs and Playwright; demonstrate them by retrieving documentation content or running a Playwright action via chat.

#### How to Use MCP Servers in Visual Studio Code

For detailed instructions on enhancing GitHub Copilot with MCP servers in Visual Studio Code, refer to the official documentation: [Customize Copilot with MCP Servers in VS Code](https://code.visualstudio.com/docs/copilot/customization/mcp-servers).

#### MS Docs

The MS Docs MCP server allows developers to access Microsoft documentation and learning resources directly within their development environment. By integrating it into VS Code, you can quickly retrieve relevant documentation, examples, and best practices while you work.

Use an MCP integration in VS Code to connect to Microsoft Docs/Learn. In Copilot Chat (Agent Mode), ask via the MS Docs MCP server: "What are the key benefits of GitHub Copilot?" Then post back using the following format:

- The canonical Microsoft Docs link used
- A 2–3 sentence summary with one short quoted line
- Bullet points of key benefits

#### Browser Automation with Playwright

Playwright is a tool that lets you automate web browsers with code. You can use it to open pages, click buttons, fill forms, and get information from websites easily.

Playwright can leverage MCP to access documentation and examples related to browser automation, making it easier for developers to implement and troubleshoot their automation scripts. By integrating with MCP, Playwright can provide context-aware suggestions and resources directly within the development environment.

Using Playwright, interact in Copilot Chat to automate Google Maps and obtain driving directions from "Piccadilly Circus, London" to "University of Surrey, Guildford." Extract and print the main route’s travel duration and distance to the console.

## Success Criteria

- ✅ GitHub Copilot configured in VS Code
- ✅ Inline code suggestions demonstrated
- ✅ Edit Mode used to refactor existing code with specific instructions
- ✅ Interaction with @workspace, @vscode, and @terminal agents demonstrated
- ✅ Appropriate use cases for each Copilot mode explained
- ✅ Plan Mode used to generate and review a task plan before execution
- ✅ MCP integration used to fetch a Microsoft Docs article
- ✅ Playwright used to automate Google Maps driving directions

## Learning Resources

- [Set up GitHub Copilot in VS Code](https://code.visualstudio.com/docs/copilot/setup)
- [GitHub Copilot in VS Code](https://code.visualstudio.com/docs/editor/artificial-intelligence)
- [Use MCP servers in VS Code](https://code.visualstudio.com/docs/copilot/customization/mcp-servers)
- [GitHub Copilot Chat Documentation](https://docs.github.com/en/copilot/using-github-copilot/asking-github-copilot-questions-in-your-ide)
- [Model Context Protocol (MCP) Overview](https://modelcontextprotocol.io/)
- [VS Code MCP (Model Context Protocol)](https://code.visualstudio.com/mcp)
- [GitHub Copilot Best Practices](https://docs.github.com/en/copilot/using-github-copilot/best-practices-for-using-github-copilot)
- [Playwright Docs](https://playwright.dev/docs/intro)
