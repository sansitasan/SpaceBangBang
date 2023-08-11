# SpaceBangBang
SpaceBangBang Game Repo

Cardlist

Bang - Shot one time

Dodge - Dodge Shot

Guard - Create a structure in front of you that can block the shot three times, if there is no leading space, can`t use card

Lazer - Get Lazergun

Machinegun - Get Machinegun

Shotgun - Get Shotgun

Sniper - Get Sniper

Recorvery - Recorvery hp + 1

Scope - It gives you a broader view. Dotween is applied when the field of view is widened

Spray - Shot in 8 directions

Steal - Steal other player`s card, if they have not cards, Draw one card

Supply - Draw two card

--------------------------
Effectlist

AttachGameObjectsToParticles - add particle(Mainly applied to bullets and guns)

CameraShake - when shot CameraShaking

DropGun - when change gun, Throw away the old guns

TextEffect - if player damaged or Dodge or stolen card, setAcive Around the player

---------------------------------------
Gunlist

Bullet - get speed, dir, coroutine and startpos, Conflict handling with player, bullet and guard

gun - base of all gun, gun have stat(cooltime, range, ShakeIntensity etc), 
          Get directions from players to determine sorting order and dir
          
K2 - default gun

lazer - special gun, when shot it charging Fixed duration and shot lazer

machinegun - Although the range is short, shot fast and Use bang with 50% probability

shotgun - Although the range is short, shot 3 bullet at a time

Sniper - it has long range

-------------------------------------------
Managerlist

CardManager - it has cardsprite, and card`s probability, return ICard and CardType

DataManager - it has firebase auth, sound action and voluume, and player, weapon stat, kotlin file
                  it Responsible for overall data and signups
                  
GameManager - it Responsible Miscellaneous stuff, Coroutine Dict, overall gametime, sortaxis, frame etc

NetworkManager - it Responsible Network(photon), Handling Game Wins and Losses, lobby, roomscene and player

ObjectPoolManager - it Responsible ObjectPool mainly effect, card

SceneManagerEx - it Responsible fade-out when SceneLoadAsync

-----------------------------------------------------------
Maplist

GuardObject - when use GuardCard, create Guard

ItemBox - An object that grants 5 cards when captured at a certain time.

MapController - has player spawner

TilemapMake - testcode

----------------------------------------------------------
Playerlist

Player - base of all player, it has two state and handler, cardhandle, ui etc
            it handle use card and shot etc
            
PlayerState - playerstate, hit, crouch, idle, move, dead and their handler

playerlookstate - The direction the player is looking

-----------------------------------------------------------
Scenelist

BattleScene - it has CinemachineVirtualCamera, mapcontroller, volume, panels
                  when battle is start, Init Player, UI and Directing
                  if player dead, player can watch the game alive player sight
                  if end game, start camera Directing and set win or draw panel
                  
LobbyScene - player can create room or join room, setting sound, if logout game goto mainscene
                  Pick a nickname one time in this scene
                  if keydown escape two times, goto maincene
                  
MainScene - StartScene, if touch screen, Enter loginscene or lobbyscene

LoginScene - player can signup wit kakao or email or anonymous

RoomScene - When two or more people are in the room and ready, select a character

------------------------------------------
Soundlist

BgmPlayer - BgmPlay all scene  

CAudio - base of all Sound

-------------------------------------------
UIlist
