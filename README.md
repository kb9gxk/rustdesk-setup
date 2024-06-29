This is my little utility to make sure that I always have the latest version of Rustdesk installed.

If you have the network configuration or want to set a permanent password, edit the Configuration.cs file before compiling.

You can also use the command line inputs of:

--stable           Use stable version of RustDesk (default if not specified)
--nightly          Use nightly version of RustDesk
--config=<value>   Set network configuration output from the Network Settings
--password=<value> Set the permanent password for RustDesk
--help             Show this help message

If you rename the compiled version to rustdesk-nightly.exe it will default to nightly.