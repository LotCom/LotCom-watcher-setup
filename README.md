# Setup Script for the LotCom Watcher application

[![Package Setup](https://github.com/LotCoM/LotCom-watcher-setup/actions/workflows/cicd.yml/badge.svg?branch=develop)](https://github.com/LotCoM/LotCom-watcher-setup/actions/workflows/cicd.yml)

Uses WiX and WiX-Sharp to create a custom MSI builder.

This builder's unpackaged release files can be side-loaded in the `Setup` folder in the LotCom Watcher application project. From here, LotCom Watcher's CI/CD pipeline will use the setup program to create and release an MSI package.

> [!Warning]
> When making changes to the Setup program, you must run `dotnet build` and migrate the newly generated release files to LotCom Watcher's `Setup` directory. 
> 
> Otherwise, LotCom Watcher will not run the modified Setup program in its CI/CD pipeline.
