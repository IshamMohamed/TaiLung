# Tai Lung
![Attacking Tai Lung](http://aux4.iconspalace.com/uploads/955657164690126020.png)

Microsoft Bot Framework v4 boilerplate to get started with Microsoft Bots aggressively as Tai Lung. This follows the singleton pattern to create the bot. 

## How To Configure Your Own Version
1. Clone the repo.
2. Change the ```BotConfiguration.bot``` file with the proper port.
3. Rename ```MainBot.cs``` and ```MainBotAccessors.cs``` as per your requiremment - or you can keep the same name. 
4. If you are planning to have custom states, navigate to States folder and add a new state, which is just a POCO. Later use the custom state in both ```MainBot.cs``` and ```MainBotAccessors.cs```.
