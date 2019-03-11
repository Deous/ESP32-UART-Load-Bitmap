
#ifndef __LCD_H_
#define __LCD_H_

#ifdef __cplusplus
extern "C" {
#endif

#include "freertos/FreeRTOS.h"
#include "driver/spi_master.h"

/*
    	    HSPI	VSPI
Pin Name    GPIO Number
------------------------
CS0 	    15	    5
SCLK	    14	    18
MISO	    12	    19
MOSI	    13	    23
*/

#define PIN_NUM_MISO 12
#define PIN_NUM_MOSI 13
#define PIN_NUM_CLK  14
#define PIN_NUM_CS   15

#define PIN_NUM_DC   21
#define PIN_NUM_RST  22
#define PIN_NUM_BCKL 5

#define DISPLAY_WIDTH  320
#define DISPLAY_HEIGHT 240

#define BLOCK_LINES (DISPLAY_HEIGHT / 2)


typedef struct {
    uint8_t cmd;
    uint8_t data[16];
    uint8_t databytes; //No of data in data; bit 7 = delay after set; 0xFF = end of cmds.
} lcd_init_cmd_t;


volatile spi_device_handle_t spi;


void initialise_lcd(void);
void display_image_block(spi_device_handle_t spi, int ypos, void *image_data);
void wait_data_done(spi_device_handle_t spi) ;

#ifdef __cplusplus
}
#endif

#endif

