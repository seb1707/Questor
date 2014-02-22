This documentation cpoied from the commit message here: https://github.com/ISeeDEDPpl/Questor/commit/81350e5f471a6776baae985531b8893a61304b81

By Default it will use "skillPlan.txt" if available in the same folder as questor.exe. If you want different Skill Plans for each toon create a skill plan for each one and name it
[ skillplan-NameOfMyToonHere.txt ]

To genereate a skill plan you can use evemon. Open the plan that you've created in evemon, at the top left you will see an export button, export Plan, Save As Type: Text Format, Save. UNCHECK ALL options (so that we get a list of skills AND NO OTHER INFO!), press ok.
I suggest you open the resulting skill plan and verify that ALL PREREQUISITE skill s are included in the plan.
If you have Caldari Frigate III listed you should make sure you also list Caldari Frigate I and Caldari Frigate II (one skill per line!).
you are not required to have skills listed you already have trained but keep in mind that you may use that same plan on another toon later and not having the prerequisites in the plan will be bad!

the template skillplan that comes with questor is for a caldari destroyer and is quite inefficient. You WILL want to change the plan contents.

Skillbooks will be pulled from both the Itemhangar or Ammohangar (however they are defined). This means you can put skillbooks for all toons in the Corporate AmmoHangar if configured.
<thisToonShouldBeTrainingSkills>false</thisToonShouldBeTrainingSkills>
needs to be true if you wish to have questor train skills (filling the skill queue every few hours as needed, between missions while docked)