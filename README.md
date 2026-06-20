# DRB-III Database Reader

This is a tool for parsing DRB-III Database file (.mem)  
and extract useful info about possible communication messages for defined ECUs.  
Note that you should bring your own database.mem file  
because it can't be shared by GPL-3.0 license.  
It comes with every DRB-III Emulator installation.  

All massive groundbreaking work has been done by [@MWIsBest](https://github.com/MWisBest) in his original repository [DRBDBReader](https://github.com/MWisBest/DRBDBReader).  

This is an **experimental** fork for experimental experiments *without any promises*.  

Intentions:
- [x] Rework project base to make possible build in lightweight VSCode with modern .NET
- [x] Provide reproducible project build by using GitHub Actions
- [ ] Rework tool to use CLI instead of GUI to make usage of this tool easier
- [ ] Implement a reliable way to link stubbed messages to processes/scripts that are using multiple messages
- [ ] Create a tool usage documentation
