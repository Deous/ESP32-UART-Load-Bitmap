## *Load an image bitmap via COM port and display it on LCD (ILI9341)*
The following example demonstrates how to load any image via UART0 (onboard USB-TTL on esp32) that is usually an onboard USB to TTL connection so it is easier for user to connect Esp device right away.

## Setup and Test Guide

- [Hardware parts](#hardware-parts)
   - [LCD 320x240 ILI9341 (SPI)](#lcd-display)
   - [ESP32 Board](#esp32-board)
- [Code of windows app and esp firmware](#code-of-windows-app-and-esp-firmware)
   - [WPF Application in C#](#wpf-application-c)
   - [IDF Project in C/C++](#idf-project)
- [Connect Pins](#connect-pins)
- [Compile code](#compile-code)

- [How it works](#how-it-works)

## Hardware Parts

### LCD Display

I used a popular TFT 32x240 ILI9341 display with SPI interface

### ESP32 Board

ESP32 module used was brown WROOM board with 4Mb flash but you can use any board
like WROVER which will increase speed of transfer if you use PSI RAM (see notes)

## Code of Windows app and ESP firmware

We essentially need a client-server or client-client communication in this case
The data from PC is sent after selecting an image from very simple picture list that points to your folder with pictures
By clicking on the picture the image is instantly sent to ESP32

### WPF Application (C#)
It is a very basic application based on WPF written in C#.NET
The solution uses Visual Studio 2017 but feel welcome to upgrade or modify it so it compiles and works

### IDF Project
The project follows code example included with idf framework. It uses CMake. The program awaits for UART event when COM data arrives from PC and sends it to TFT via SPI pins. 

## Connect Pins


## Compile Code


## Run and Test

