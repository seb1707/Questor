:: Set-ExecutionPolicy
::
::Change the user preference for the execution policy of the shell.
::
::Syntax
::      Set-ExecutionPolicy [-executionPolicy] Policy
::        { Unrestricted | RemoteSigned | AllSigned | Restricted | Default | Bypass | Undefined}
::            [[-Scope] ExecutionPolicyScope ] [-Force]
::               [-whatIf] [-confirm] [CommonParameters]
::
::Key
::   -ExecutionPolicy Policy
::       A new execution policy for the shell.
::
::       Valid values:
::        
::       Restricted
::       Do not load configuration files or run scripts.
::       This is the default.
::        
::       AllSigned
::       Require that all scripts and configuration files be signed
::       by a trusted publisher, including scripts that you write on the
::       local computer.
::        
::       RemoteSigned
::       Require that all scripts and configuration files downloaded
::       from the Internet be signed by a trusted publisher.
::        
::       Unrestricted
::       Load all configuration files and run all scripts.
::       If you run an unsigned script that was downloaded from the
::       internet, you are prompted for permission before it runs.
::
::       Bypass
::       Nothing is blocked and there are no warnings or prompts.
::        
::       Undefined
::       Remove the currently assigned execution policy from the
::       current scope. This parameter will not remove an execution
::       policy that is set in a Group Policy scope.
::
::   -Force
::       Suppress all prompts.
::       By default, Set-ExecutionPolicy displays a warning whenever the
::       execution policy is changed.
::        
::    -Scope ExecutionPolicyScope
::       The scope of the execution policy.
::        
::       Valid values:
::         Process       Affect only the current PowerShell process.
::         CurrentUser   Affect only the current user.
::         LocalMachine  Affect all users of the computer.
::        
::       To remove an execution policy from a particular scope, set the
::       execution policy for that scope to Undefined.
::
::   -WhatIf
::       Describe what would happen if you executed the command without actually
::       executing the command.
::        
::   -Confirm
::       Prompt for confirmation before executing the command.
::
::   CommonParameters:
::       -Verbose, -Debug, -ErrorAction, -ErrorVariable, -WarningAction, -WarningVariable,
::       -OutBuffer -OutVariable.
::

Set-ExecutionPolicy -ExecutionPolicy Unrestricted