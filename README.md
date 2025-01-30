# Rustdesk Auto-Installer

This utility automates the download and installation of the latest Rustdesk client, either the stable or nightly build. It also provides options for configuring the network settings and setting a permanent password via command-line arguments.

## Features

*   **Automated Installation:** Downloads and installs the latest Rustdesk version (stable or nightly).
*   **Version Selection:** Choose between stable and nightly builds using command-line arguments or by naming the executable.
*   **Configuration:** Set network configuration and permanent password via command-line arguments.
*   **Shortcut Removal:** Automatically removes the desktop shortcut created by the installer.
*   **Logging:** Creates a detailed log file of the installation process.
*   **Rustdesk ID Retrieval:** Automatically retrieves and saves the Rustdesk ID.
*   **Popup Notification:** Displays a message box with the computer name, Rustdesk ID, and version after installation.
*   **Cleanup:** Removes the downloaded installer executable after successful installation.

## Usage

The installer can be used in two ways:

1.  **Default Behavior:**
    *   If the executable is named `rustdesk-nightly.exe` (or starts with `rustdesk-nightly`), it will default to installing the nightly version.
    *   Otherwise, it will default to installing the stable version.
    *   The executable will install Rustdesk silently, remove the desktop shortcut, and display a popup with the Rustdesk ID.

2.  **Command-Line Arguments:**
    You can customize the installation by using the following command-line arguments:

    ```
    <executable_name> [--stable|--nightly] [--config=<value>] [--password=<value>] [--help]
    ```

    *   `--stable`: Forces the installer to use the stable version of Rustdesk.
    *   `--nightly`: Forces the installer to use the nightly version of Rustdesk.
    *   `--config=<value>`: Sets the network configuration string output from the Rustdesk Network Settings.
    *   `--password=<value>`: Sets the permanent password for Rustdesk.
    *   `--help`: Displays this help message with usage instructions.

    **Examples:**

    *   To install the stable version:
        ```
        rustdesk-installer.exe --stable
        ```
    *   To install the nightly version with a specific configuration and password:
        ```
        rustdesk-installer.exe --nightly --config="someConfigValue" --password="somePassword"
        ```
    *   To display the help message:
         ```
        rustdesk-installer.exe --help
        ```

## Configuration

*   **Default Settings:** You can modify the default network configuration and password by editing the `Configuration.cs` file before compiling.
*   **Log File:** The installer creates a log file at `c:\Rustdesk-<Stable/Nightly>-Install.log`.
*   **Rustdesk Info File:** The installer saves the computer name and Rustdesk ID to `c:\rustdesk.txt`.

## How to Build

1.  Clone or download the repository.
2.  Open the solution in Visual Studio or another compatible IDE.
3.  Build the project.
4.  The resulting executable will be in the `bin\Debug` or `bin\Release` folder.

## Important Notes

*   The installer requires an internet connection to download Rustdesk.
*   The installer requires administrative privileges to install Rustdesk.
*   The network configuration string must be copied from the Rustdesk Network Settings.
*   The permanent password will be set for the Rustdesk client after installation.
*   The installer will remove the desktop shortcut created by the Rustdesk installer.

## License

This project is licensed under the GNU General Public License v3. See the `LICENSE.txt` file for details.

## Contributing

Feel free to submit pull requests or open issues to contribute to this project.# Rustdesk Auto-Installer

This utility automates the download and installation of the latest Rustdesk client, either the stable or nightly build. It also provides options for configuring the network settings and setting a permanent password via command-line arguments.

## Features

*   **Automated Installation:** Downloads and installs the latest Rustdesk version (stable or nightly).
*   **Version Selection:** Choose between stable and nightly builds using command-line arguments or by naming the executable.
*   **Configuration:** Set network configuration and permanent password via command-line arguments.
*   **Shortcut Removal:** Automatically removes the desktop shortcut created by the installer.
*   **Logging:** Creates a detailed log file of the installation process.
*   **Rustdesk ID Retrieval:** Automatically retrieves and saves the Rustdesk ID.
*   **Popup Notification:** Displays a message box with the computer name, Rustdesk ID, and version after installation.
*   **Cleanup:** Removes the downloaded installer executable after successful installation.

## Usage

The installer can be used in two ways:

1.  **Default Behavior:**
    *   If the executable is named `rustdesk-nightly.exe` (or starts with `rustdesk-nightly`), it will default to installing the nightly version.
    *   Otherwise, it will default to installing the stable version.
    *   The executable will install Rustdesk silently, remove the desktop shortcut, and display a popup with the Rustdesk ID.

2.  **Command-Line Arguments:**
    You can customize the installation by using the following command-line arguments:

    ```
    <executable_name> [--stable|--nightly] [--config=<value>] [--password=<value>] [--help]
    ```

    *   `--stable`: Forces the installer to use the stable version of Rustdesk.
    *   `--nightly`: Forces the installer to use the nightly version of Rustdesk.
    *   `--config=<value>`: Sets the network configuration string output from the Rustdesk Network Settings.
    *   `--password=<value>`: Sets the permanent password for Rustdesk.
    *   `--help`: Displays this help message with usage instructions.

    **Examples:**

    *   To install the stable version:
        ```
        rustdesk-installer.exe --stable
        ```
    *   To install the nightly version with a specific configuration and password:
        ```
        rustdesk-installer.exe --nightly --config="someConfigValue" --password="somePassword"
        ```
    *   To display the help message:
         ```
        rustdesk-installer.exe --help
        ```

## Configuration

*   **Default Settings:** You can modify the default network configuration and password by editing the `Configuration.cs` file before compiling.
*   **Log File:** The installer creates a log file at `c:\Rustdesk-<Stable/Nightly>-Install.log`.
*   **Rustdesk Info File:** The installer saves the computer name and Rustdesk ID to `c:\rustdesk.txt`.

## How to Build

1.  Clone or download the repository.
2.  Open the solution in Visual Studio or another compatible IDE.
3.  Build the project.
4.  The resulting executable will be in the `bin\Debug` or `bin\Release` folder.

## Important Notes

*   The installer requires an internet connection to download Rustdesk.
*   The installer requires administrative privileges to install Rustdesk.
*   The network configuration string must be copied from the Rustdesk Network Settings.
*   The permanent password will be set for the Rustdesk client after installation.
*   The installer will remove the desktop shortcut created by the Rustdesk installer.

## License

This project is licensed under the GNU General Public License v3. See the `LICENSE.txt` file for details.

## Contributing

Feel free to submit pull requests or open issues to contribute to this project.
