/* UART Load Bitmap Example
    Below code loads a butmap image sent from Windows client
    after reading bytes in form of TFT 

   This example code is in the Public Domain (or CC0 licensed, at your option.)

   Unless required by applicable law or agreed to in writing, this
   software is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
   CONDITIONS OF ANY KIND, either express or implied.
*/
#include <stdio.h>
#include "freertos/FreeRTOS.h"
#include "freertos/task.h"
#include "driver/uart.h"
#include "esp_log.h"
#include "lcd.h"

// this is an LCD line block size to be displayed by one iteration of spi
#define BUF_SIZE (DISPLAY_WIDTH*2*BLOCK_LINES)

// we substitute esp log functions with our own
int printf_func(const char* str, va_list vl)
{
    return 0;
}

static QueueHandle_t uart0_queue;

static void uart_task()
{
    uart_event_t event;
    int n_received=0, n_total=0;

    uint8_t *data = (uint8_t *)malloc(BUF_SIZE);

    for(;;)
    {
        if(xQueueReceive(uart0_queue, (void*)&event, portMAX_DELAY))
        {
            if(event.type==UART_DATA)
            {
                for (int y=0; y<DISPLAY_HEIGHT; y+=BLOCK_LINES)
                {
                    n_received=0;
                    n_total=0;

                    while(n_total != BUF_SIZE)
                    {
                        n_received = uart_read_bytes(UART_NUM_0, data + n_total, BUF_SIZE, portMAX_DELAY);
                        n_total += n_received;
                    }

                    display_image_block(spi, y, data);
                    wait_data_done(spi);
                }
                uart_flush_input(UART_NUM_0);
            }

        }
    }
}

void app_main()
{
    initialise_lcd();
    esp_log_set_vprintf(printf_func);

    uart_config_t uart_config = {
        .baud_rate = 115200,
        .data_bits = UART_DATA_8_BITS,
        .parity    = UART_PARITY_DISABLE,
        .stop_bits = UART_STOP_BITS_1,
        .flow_ctrl = UART_HW_FLOWCTRL_DISABLE
    };

    uart_param_config(UART_NUM_0, &uart_config);
    uart_driver_install(UART_NUM_0, BUF_SIZE, 0, 2, &uart0_queue, 0);

    xTaskCreate(uart_task, "uart_task", 1024, NULL, 10, NULL);

    //printf("\n\nUART(0) TASK STARTED~~~~~~~~~~\n\n");
}
