# Challenge 00 - Setup and prepare Evironment

**[Home](../README.md)** - [Next Challenge >](./Challenge-01.md)

## Pre-requisites

- Your laptop (development machine): Win, MacOS or Linux that you have **administrator rights**.
- Active Azure Subscription with **Contributor access** to create or modify resources.
- Access to Azure OpenAI in the desired Azure subscription.
- Latest version of Azure CLI
- Latest version of Visual Studio or Visual Studio Code
- .NET 9.0 SDK or later version
- GitHub Copilot (free tier with limitations available, or start a Copilot Pro trial for enhanced features)

## Introduction

In this session you will setup your computer and cloud environment with the minimum required tools.

## Description

Setup and configure the following tools

- Use your active Azure Subscription or the one provided for the hackathon.
- Log into the [Azure Portal](https://portal.azure.com) and confirm that you have an active subscription that you can deploy cloud services.
- Use the latest version of [Visual Studio](https://visualstudio.microsoft.com) or [Visual Studio Code](https://code.visualstudio.com) if you don't have it.n
- Install .NET .0 SDK or later version
- Set up [GitHub Copilot](https://github.com/features/copilot/plans) - if you don't have a license, sign up for the free tier or start a Copilot Pro trial for enhanced AI assistance during development
- Install Playwright for .NET by running: `dotnet tool install --global Microsoft.Playwright.CLI`
- Clone the GitHub repository onto your workstation.

### Practical Exercise: Test Playwright with Google Maps

Create a simple Playwright script to automate getting driving directions:

- Create a new console application
- Use Playwright to navigate to Google Maps
- Search for directions from "London Piccadilly Circus" to "University of Surrey, Guildford"
- Extract and display the estimated driving time and distance
- Take a screenshot of the directions

## Success Criteria

- You should be able to log in to the Azure Portal.
- You have a bash shell or PowerShell at your disposal (you can also use Azure Cloud Shell)
- Running az --version shows the version of your Azure CLI
- Visual Studio or Visual Studio Code is installed.
- Playwright CLI is installed and functional
- Successfully created and ran a Playwright script that gets driving directions from London Piccadilly Circus to University of Surrey in Guildford
- Ensure that you clone the GitHub repository onto your workstation.
