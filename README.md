<div align="center">

[![Version](https://img.shields.io/github/v/release/XKaguya/AutoReconnect-Remastered?sort=semver&style=flat-square&color=8DBBE9&label=Version)]()
[![GitHub Issues](https://img.shields.io/github/issues/XKaguya/AutoReconnect-Remastered/total?style=flat-square&label=Issues&color=d77982)](https://github.com/XKaguya/AutoReconnect-Remastered)
![Downloads](https://img.shields.io/github/downloads/XKaguya/AutoReconnect-Remastered/total?style=flat-square&label=Downloads&color=d77982)

</div>

# StoPasswordBook
A tool used to conveniently change accounts.

# This program will store sensitive data (Passwords) to local files. Please do not spread your `Shadow.xml` to anyone else!!!

<img width="594" alt="bc1fd56ad8dfaf8e2f239506353f34ad" src="https://github.com/user-attachments/assets/3b73681f-df1f-4bc7-b8f2-c80e24f617d7">


# Usage

* Download the latest release version from [Releases](https://github.com/XKaguya/StoPasswordBook/releases/latest).
* Double-click the program to run it. Wait until Chrome is installed. (At this stage, it might fail due to network issues. Check the logs if nothing happens and it's stuck.)
* Wait until the text block turns green with the message `Done.`
  
  ![screenshot](https://github.com/user-attachments/assets/fe4e97a7-7087-4825-8f0d-f073bcd58962)

Once `Done.` appears, you are free to choose which account and password will be input into the `Launcher`.

* The accounts and passwords are defined in the `Shadow.xml` file, which will be automatically created with the following example:
  
  ```xml
  <?xml version="1.0" encoding="utf-8"?>
  <Accounts>
    <Account>
      <Name>Account1</Name>
      <Password>Password1</Password>
    </Account>
    <Account>
      <Name>Account2</Name>
      <Password>Password2</Password>
    </Account>
    <Account>
      <Name>Account3</Name>
      <Password>Password3</Password>
    </Account>
  </Accounts>
  ```

* If you encounter a situation where it gets stuck at `Config Saved`, simply click the submit button again. The program will automatically close all existing STO Launcher instances and start a new one.
* Please note that this program only supports launchers started by itself.

# How the Launcher accepts external input
Star Trek Online Launcher is a `CEF` Program. By using the `--remote-debugging-port=port` command, it allows users to remotely debug with DevTools.

# Contributing
Contributions are welcome! Feel free to submit [issues](https://github.com/XKaguya/StoPasswordBook/issues) or [pull requests](https://github.com/XKaguya/StoPasswordBook/pulls) to improve the project.
