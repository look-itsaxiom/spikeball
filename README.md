## What is Spikeball?

a simple, small game that my kiddo came up with on a whim that I decided to appropriate into something I can use to practice game development in Godot.

The game is a local multiplayer 1vGroup style and goes like this:
The game is conducted in rounds
Each round, 1 player is the "ball" and the other players are the "spike balls"
The ball player's goal is to get itself into the "hoop" on the other side of the screen before time runs out
The spike ball players' goal is to collide with the ball to destroy it, or otherwise prevent the ball from going into the hoop within the time
If the ball player makes it into the hoop, that player gains points equal to the number of other players in the game
If the spikeball players stop the ball player from making it to the hoop, each of them gets 1 point
The round ends when the ball enters the hoop successfully, the spike ball players destroy the ball, or the round timer runs out
In the next round, the next player becomes the ball and the previous becomes a spikeball. Rotating the ball player every round.
This continues until a player reaches 10 points, making them the winner of the game.

## Project Scope

I'm trying very hard not to think to hard about this game. I want to implement it exactly as it is defined and not let myself get carried away adding more and more things.
There may be things to tweak to make the game a little more fun like the spikeballs moving slightly slower than the ball,
or giving the ball a "boost" power to enable quick bursts of movement but again, I really don't want to get scope creep because I want to **finish** this.

Alright, lets define what "finish" means.

- An opening menu scene
  - simple options of "start game" and "exit"
  - start game should bring you to a sub menu where you can select the number of players going to play
- A game loading scene that selects which player will be the ball player first randomly
- The game itself, playable by 1-4 players connected by various controllers (local multiplayer only)
  - A count down timer to start the round
  - Score display at top of screen
  - A round end screen showing points incrementing and the ball player being changed
  - A winner winner game over screen with the option to play again

I think that's pretty much it without going into specific game design details

## Technology Used

- Godot 4.4.1 .NET
- Aesprite / Krita for art

## But Why?

I haven't been able to follow the "make small games" advice in the past so this is my attempt at that :)
This game should teach me the following unknown skills: - local multiplayer handling - finishing a game and distribution - music and sfx - building art skills - anything else that I don't know I don't know yet
Alright enough planning, lets get to it!
