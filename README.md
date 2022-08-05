# gmcchallengejobsity

## How to build and run

The exercise was developes using C# with .Net 6.0 
To build and run you must have .NET Runtime 6.0 (or greater) instaled 
(https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
After unzip the file sent, switch to the project directory 
JobsityNetChallenge, which contains the file 
JobsityNetChallenge.csproj

And run the command 
>dotnet run 

By default the application run at port 5000, to access the aplication open the link http://localhost:5000 on a browser.

## External dependencies
### RabbitMQ
The application contains a decoupled bot the gather stock prices per user request, so it relies on a message brokers named RabbitMQ (www.rabbitmq.com), to send messages asynchornous back to the user.

The Host name ("Hostname"), port ("Port"), username ("User") and password ("Password") of RabbitMQ service must be configured at appsettings.json that is JobsityNetChallenge at directory.

The configuration keys are:
```
"RabbitMq": {
	"QueueName": "orders", //It is recommend that you do not change
	"RouteKey": "stockquote", //It is recommend that you do not change
	"Hostname": "amqps://XXXXX", //change to your rabbitmq service hostname or ip
	"Port": 5672 //change to your rabbitmq service port
	"User": "XXXXX", //change to your rabbitmq service username
	"Password": "XXXXX" //change to your rabbitmq service password
}
```

### LiteDb
The aplication also relies on a database to save message history and user information, I choose to use a local serverless NoSQL database 

At the appsettings.json configuration file you must specify the database location:

"DataBaseConfig": "LiteDb/chatPersistence.db"

The database file is locate under LiteDb directory of application JobsityNetChallenge directory

So it is not recommended that you change the configuration file.

## Design decisions and documentation

### Registered users
To allow registered users to log in, I used the database to save and provide thos users, the password is stored hash encrypted, so confidential information is secured.

The user and their password are:
user1/test
user2/test
user3/test

A simple API was provided if any additional user must be inserted, or the test database is discarted, where the user name and password to be created are provided at the url:

http://localhost:5000/auth/Register/{user}/{password}

The database access to read and write data are developed in classes under JobsityNetChallenge.Storage project

### Local storage and previous messages
The database also records every message sent using the chat, when the user logs in, the system retrieve the last 50 messages and show to the user orderer by their timestamp

### Stock Bot
Communication with stock bot was implemented asynchronous when a user command is received a fire and forget Task is called a class (implemented at JobsityNetChallenge.StockBot package) that calls the external API to gather stocl info. So the user is informed that the stock price will be gathered in a few moments, and his connection with the server is not halted waiting for the result.

This client call the external API, process the result as a CSV file, and enqueue the result using RabbitMQ service. If the stock was not found, it warns the user of this problem.

To send the stock gathered back to user, a HostedService was implemented at the class RabbitListener, It recevies messages posted at RabbitMQ, process the result, sends back the information to the user at the chat room.

So:
<User command> ---assync--> StockBotClient ---enqueue--> RabbitMQRabbitMQ 

RabbitMQRabbitMQ <--Dequeue--- RabbitListener -----> chat room


### Chat functionalities
All functionalities to implement the chat are developed using SignalR library, it keeps the connection open between the browser and the server, is is also able to detect when a user is connected and disconnected.

It is responsible to receive user messages and using it javascript library and post to the server, server process and sends back to the chat, using SignalR libraries also, it allows to send message to all users, or to a single user (used when a stock command is received)

At the server side all chat functionalities were implemented at ChatMessageHub class.


### How user is identified
When the user logs, it is saved on the database the current connection Id identified by the SignalR.

Stock gathered also enqueue the user data that post the command, when the message is processed it has the user SignalR connection Id to send the message back to the user.

Session was used to store current logged user, enabling reload of the chat web page, so multiple tabs can not be open using the same browser, to simulate more than one user at the same computer another browser or a anonymous tab must be used.
