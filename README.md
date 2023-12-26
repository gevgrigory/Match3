Match3 Project

Simple match3 project, that includes match3 mechanics like destroying the same color tiles which occur 3+ in a row both horizontally and vertically,
also with a square pattern. It has dynamic grid settings and gems variety count settings.
Currently, there are 5 different gem textures, but in case you choose more variety - there is a generic item implemented to support as many varieties as you set,
by changing the hue color.

<img width="331" alt="Screenshot 2023-12-26 at 3 24 29 PM" src="https://github.com/gevgrigory/Match3/assets/78232344/fa9cf04c-3cdb-4879-8edf-cf1678fa2dde">

Here is some part from the gameplay:

https://github.com/gevgrigory/Match3/assets/78232344/6de3566f-1012-4975-9835-544d6167a44e

Also, there is an ability to autoPlay, in case it can help developers or testers for better testing, or just for adding hints functionality later.

https://github.com/gevgrigory/Match3/assets/78232344/f3add346-814e-48c2-996c-6bdadfaf8dc6

There is also an option to run a simulation of player moves by count, to check the level design and balance, or find some issues during development.
Within the simulation player moves can be fully randomized, or the ones, that lead to matching the gems.
The simulation is done in a separate thread to not freeze the game, so the developer will have the ability to profile the process.

https://github.com/gevgrigory/Match3/assets/78232344/9b75ac93-4b4a-4def-a6c1-b89041a912bf

