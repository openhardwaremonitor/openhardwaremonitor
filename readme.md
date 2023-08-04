The Open Hardware Monitor is a free open source application that monitors temperature sensors, fan speeds, voltages, load and clock speeds of a computer.

## Supported Hardware

-   CPU core sensors
    -   Intel Core 2, Core i3/i5/i7, Atom, Sandy Bridge, Ivy Bridge, Haswell, Broadwell, Silvermont, Skylake, Kaby Lake, Airmont, Goldmont, Goldmont Plus, Cannon Lake, Ice Lake, Comet Lake, Tremont, Tiger Lake
    -   AMD K8 (0Fh family), K10 (10h, 11h family), Llano (12h family), Fusion (14h family), Bulldozer (15h family), Jaguar (16h family), Puma (16h family), Ryzen (17h, 19h family)
-   Mainboard sensors
    -   ITE IT8620E, IT8628E, IT8655E, IT8665E, IT8686E, IT8688E, IT8705F, IT8712F, IT8716F, IT8718F, IT8720F, IT8721F, IT8726F, IT8728F, IT8771E, IT8772E, IT8792E/IT8795E
    -   Fintek F71808E, F71858, F71862, F71868AD, F71869, F71869A, F71882, F71889ED, F71889AD, F71889F
    -   Nuvoton NCT6102D, NCT6106D, NCT6771F, NCT6772F, NCT6775F, NCT6776F, NCT6779D, NCT6791D, NCT6792D, NCT6792D-A, NCT6793D, NCT6795D, NCT6796D, NCT6796D-R, NCT6797D, NCT6798D
    -   Winbond W83627DHG, W83627DHG-P, W83627EHF, W83627HF, W83627THF, W83667HG, W83667HG-B, W83687THF
-   GPU sensors
    -   Nvidia
    -   AMD (ATI)
-   Hard drives
    -   S.M.A.R.T. temperature sensors
    -   SSD wear level, host reads/writes
-   Fan controllers
    -   T-Balancer bigNG
    -   Alphacool Heatmaster

## Data Interface

The Open Hardware Monitor publishes all sensor data to WMI (Windows Management Instrumentation). This allows other applications to read and use the sensor information as well. A preliminary documentation of the interface can be found [here](http://openhardwaremonitor.org/wordpress/wp-content/uploads/2011/04/OpenHardwareMonitor-WMI.pdf).

Read more over at: https://openhardwaremonitor.org/
