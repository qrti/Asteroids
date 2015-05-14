/************************************************************************************************

Asteroids.cs


  Keyboard Controls:

  S            - Start Game    P           - Pause Game
  Cursor Left  - Rotate Left   Cursor Up   - Fire Thrusters
  Cursor Right - Rotate Right  Cursor Down - Fire Retro Thrusters
  Spacebar     - Fire Cannon   H           - Hyperspace
                               D           - Toggle Graphics Detail

************************************************************************************************/

using System;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Diagnostics;   // debug output

namespace oled
{
/************************************************************************************************
  The AsteroidsSprite class defines a game object, including it's shape, position, movement and
  rotation. It also can detemine if two objects collide.
************************************************************************************************/

public class AsteroidsSprite
{
    public static int width;              // Dimensions of the graphics area.
    public static int height;

    public Polygon shape;                 // Initial sprite shape, centered at the origin (0,0).
    public bool    active;                // Active flag.
    public double  angle;                 // Current angle of rotation.
    public double  deltaAngle;            // Amount to change the rotation angle.
    public double  currentX, currentY;    // Current position on screen.
    public double  deltaX, deltaY;        // Amount to change the screen position.
    public Polygon sprite;                // Final location and shape of sprite after applying rotation and
    // moving to screen position. Used for drawing on the screen and
    // in detecting collisions.

    public AsteroidsSprite()
    {
        this.shape = new Polygon();
        this.active = false;
        this.angle = 0.0;
        this.deltaAngle = 0.0;
        this.currentX = 0.0;
        this.currentY = 0.0;
        this.deltaX = 0.0;
        this.deltaY = 0.0;
        this.sprite = new Polygon();
    }

    public void advance()
    {
        // Update the rotation and position of the sprite based on the delta values. If the sprite
        // moves off the edge of the screen, it is wrapped around to the other side.
        this.angle += this.deltaAngle;

        if(this.angle < 0)
            this.angle += 2 * Math.PI;

        if(this.angle > 2 * Math.PI)
            this.angle -= 2 * Math.PI;

        this.currentX += this.deltaX;

        if(this.currentX < -width / 2)
            this.currentX += width;

        if(this.currentX > width / 2)
            this.currentX -= width;

        this.currentY -= this.deltaY;

        if(this.currentY < -height / 2)
            this.currentY += height;

        if(this.currentY > height / 2)
            this.currentY -= height;
    }

    public void render()
    {
        int i;

        // Render the sprite's shape and location by rotating it's base shape and moving it to
        // it's proper screen position.
        this.sprite = new Polygon();

        for(i=0; i<this.shape.npoints; i++)
            this.sprite.addPoint((int)Math.Round(this.shape.xpoints[i] * Math.Cos(this.angle) + this.shape.ypoints[i] * Math.Sin(this.angle)) + (int)Math.Round(this.currentX) + width / 2,
                                 (int)Math.Round(this.shape.ypoints[i] * Math.Cos(this.angle) - this.shape.xpoints[i] * Math.Sin(this.angle)) + (int)Math.Round(this.currentY) + height / 2);
    }

    public bool isColliding(AsteroidsSprite s)
    {
        int i;

        // Determine if one sprite overlaps with another, i.e., if any vertice
        // of one sprite lands inside the other.
        for(i=0; i<s.sprite.npoints; i++)
            if(this.sprite.inside(s.sprite.xpoints[i], s.sprite.ypoints[i]))
                return true;

        for(i=0; i<this.sprite.npoints; i++)
            if(s.sprite.inside(this.sprite.xpoints[i], this.sprite.ypoints[i]))
                return true;

        return false;
    }
}

/************************************************************************************************
  Main code.
************************************************************************************************/

public class Asteroids
{
    const int DELAY     = 50;           // Milliseconds between screen updates.
    const int MAX_SHIPS =  3;           // Starting number of ships per game.

    const int MAX_SHOTS =  6;          // Maximum number of sprites for photons,
    const int MAX_ROCKS =  8;          // asteroids and explosions.
    const int MAX_SCRAP = 20;

    const int SCRAP_COUNT = 30;        // Counter starting values.
    const int HYPER_COUNT = 60;
    const int STORM_PAUSE = 30;
    const int UFO_PASSES  =  3;

    const int MIN_ROCK_SIDES =  8;     // Asteroid shape and size ranges.
    const int MAX_ROCK_SIDES = 12;
    //const int MIN_ROCK_SIZE  = 20;
    //const int MAX_ROCK_SIZE  = 40;
    //const int MIN_ROCK_SPEED =  2;
    //const int MAX_ROCK_SPEED = 12;

    const int BIG_POINTS    =  25;     // Points for shooting different objects.
    const int SMALL_POINTS  =  50;
    const int UFO_POINTS    = 250;
    const int MISSLE_POINTS = 500;

    const int NEW_SHIP_POINTS = 5000;  // Number of points needed to earn a new ship.
    const int NEW_UFO_POINTS  = 2750;  // Number of points between flying saucers.

    int mirosi, marosi;   // min and max rocksize
    int mirosp, marosp;   // min and max rockspeed

    // Background stars.
    int     numStars;
    Point[] stars;

    // Game data.
    int score;
    int highScore;
    int newShipScore;
    int newUfoScore;

    bool running;
    bool paused;
    bool playing;
    bool detail;
    bool initialize;

    // Key flags.
    bool left  = false;
    bool right = false;
    bool up    = false;
    bool down  = false;

    // Sprite objects.
    AsteroidsSprite   ship;
    AsteroidsSprite   ufo;
    AsteroidsSprite   missle;
    AsteroidsSprite[] photons    = new AsteroidsSprite[MAX_SHOTS];
    AsteroidsSprite[] asteroids  = new AsteroidsSprite[MAX_ROCKS];
    AsteroidsSprite[] explosions = new AsteroidsSprite[MAX_SCRAP];

    // Ship data.
    int shipsLeft;       // Number of ships left to play, including current one.
    int shipCounter;     // Time counter for ship explosion.
    int hyperCounter;    // Time counter for hyperspace.

    // Photon data.
    int[] photonCounter = new int[MAX_SHOTS];    // Time counter for life of a photon.
    int   photonIndex;                           // Next available photon sprite.

    // Flying saucer data.
    int ufoPassesLeft;    // Number of flying saucer passes.
    int ufoCounter;       // Time counter for each pass.

    // Missle data.
    int missleCounter;    // Counter for life of missle.

    // Asteroid data.
    bool[] asteroidIsSmall = new bool[MAX_ROCKS];   // Asteroid size flag.
    int    asteroidsCounter;                        // Break-time counter.
    int    asteroidsSpeed;                          // Asteroid speed.
    int    asteroidsLeft;                           // Number of active asteroids.

    // Explosion data.
    int[] explosionCounter = new int[MAX_SCRAP];  // Time counters for explosions.
    int   explosionIndex;                         // Next available explosion sprite.

    Random rnd;
    Font font;
    int fontWidth;
    int fontHeight;
    int fwidth, fheight;
    keyState key;
    Form1 form;

    public Asteroids(Form1 form)
    {
        this.form = form;
        key = new keyState();
        rnd = new Random();
    }

    public void reinit()
    {
        initialize = true;
    }

    public void init()
    {
        int i;

        fwidth = form.ClientRectangle.Width;
        fheight = form.ClientRectangle.Height;

        mirosi = Math.Min(fwidth, fheight) / 20;
        marosi = mirosi * 2;

        mirosp = mirosi>14 ? 2 : 1;
        marosp = mirosi>14 ? 12 : 4;

        font = new Font("Lucida Console", mirosi>14 ? 10 : 8);
        fontWidth = TextRenderer.MeasureText("W", font).Width;
        fontHeight = (int)font.GetHeight();

        // Set the values for sprites.
        AsteroidsSprite.width = fwidth;
        AsteroidsSprite.height = fheight;

        // Generate starry background.
        numStars = AsteroidsSprite.width * AsteroidsSprite.height / 5000;
        stars = new Point[numStars];

        for(i=0; i<numStars; i++)
            stars[i] = new Point((int)(rnd.NextDouble()*AsteroidsSprite.width), (int)(rnd.NextDouble()*AsteroidsSprite.height));

        // Create shape for the ship sprite.
        ship = new AsteroidsSprite();

        if(mirosi > 14){
            ship.shape.addPoint(0, -10);
            ship.shape.addPoint(7, 10);
            ship.shape.addPoint(-7, 10);
        } 
        else{
            ship.shape.addPoint(0, -4);
            ship.shape.addPoint(2, 4);
            ship.shape.addPoint(-2, 4);
        }

        // Create shape for the photon sprites
        if(mirosi > 14){
            for(i=0; i<MAX_SHOTS; i++){
                photons[i] = new AsteroidsSprite();
                photons[i].shape.addPoint(1, 1);
                photons[i].shape.addPoint(1, -1);
                photons[i].shape.addPoint(-1, 1);
                photons[i].shape.addPoint(-1, -1);
            }
        } 
        else{
            for(i=0; i<MAX_SHOTS; i++){
                photons[i] = new AsteroidsSprite();
                photons[i].shape.addPoint(0, 0);
                photons[i].shape.addPoint(1, 0);
                photons[i].shape.addPoint(1, 1);
                photons[i].shape.addPoint(0, 1);
            }
        }

        // Create shape for the flying saucer.
        ufo = new AsteroidsSprite();

        if(mirosi > 14){
            ufo.shape.addPoint(-15, 0);
            ufo.shape.addPoint(-10, -5);
            ufo.shape.addPoint(-5, -5);
            ufo.shape.addPoint(-5, -9);
            ufo.shape.addPoint(5, -9);
            ufo.shape.addPoint(5, -5);
            ufo.shape.addPoint(10, -5);
            ufo.shape.addPoint(15, 0);
            ufo.shape.addPoint(10, 5);
            ufo.shape.addPoint(-10, 5);
        } 
        else{
            ufo.shape.addPoint(-7, 0);
            ufo.shape.addPoint(-5, -2);
            ufo.shape.addPoint(-2, -2);
            ufo.shape.addPoint(-2, -4);
            ufo.shape.addPoint(2, -4);
            ufo.shape.addPoint(2, -2);
            ufo.shape.addPoint(5, -2);
            ufo.shape.addPoint(7, 0);
            ufo.shape.addPoint(5, 2);
            ufo.shape.addPoint(-5, 2);
        }

        // Create shape for the guided missle.
        missle = new AsteroidsSprite();

        if(mirosi > 14){
            missle.shape.addPoint(0, -4);
            missle.shape.addPoint(1, -3);
            missle.shape.addPoint(1, 3);
            missle.shape.addPoint(2, 4);
            missle.shape.addPoint(-2, 4);
            missle.shape.addPoint(-1, 3);
            missle.shape.addPoint(-1, -3);
        } 
        else{
            missle.shape.addPoint(0, -2);
            missle.shape.addPoint(1, -1);
            missle.shape.addPoint(1, 1);
            missle.shape.addPoint(2, 2);
            missle.shape.addPoint(-2, 2);
            missle.shape.addPoint(-1, 1);
            missle.shape.addPoint(-1, -1);
        }

        // Create asteroid sprites.
        for(i=0; i<MAX_ROCKS; i++)
            asteroids[i] = new AsteroidsSprite();

        // Create explosion sprites.
        for(i=0; i<MAX_SCRAP; i++)
            explosions[i] = new AsteroidsSprite();

        // Initialize game data and put us in 'game over' mode.
        highScore = 0;
        detail = true;
        initGame();
        endGame();
        initialize = false;
    }

    public void initGame()
    {
        // Initialize game data and sprites.
        score = 0;
        shipsLeft = MAX_SHIPS;
        asteroidsSpeed = mirosp;
        newShipScore = NEW_SHIP_POINTS;
        newUfoScore = NEW_UFO_POINTS;
        initShip();
        initPhotons();
        stopUfo();
        stopMissle();
        initAsteroids();
        initExplosions();
        playing = true;
        paused = false;
    }

    public void endGame()
    {
        // Stop ship, flying saucer, guided missle and associated sounds.
        playing = false;
        stopShip();
        stopUfo();
        stopMissle();
    }

    public void stop()
    {
        running = false;
    }

    public void run()
    {
        long startTime = DateTime.Now.Ticks / 10000;    // 100 ns -> 1 ms
        running = true;
       
        // This is the main loop.
        while(running){
            checkKeys();

            if(initialize)
                init();

            if(!paused){
                // Move and process all sprites.
                updateShip();
                updatePhotons();
                updateUfo();
                updateMissle();
                updateAsteroids();
                updateExplosions();

                // Check the score and advance high score, add a new ship or start the flying
                // saucer as necessary.
                if(score > highScore)
                    highScore = score;

                if(score > newShipScore){
                    newShipScore += NEW_SHIP_POINTS;
                    shipsLeft++;
                }

                if(playing && score>newUfoScore && !ufo.active){
                    newUfoScore += NEW_UFO_POINTS;
                    ufoPassesLeft = UFO_PASSES;
                    initUfo();
                }

                // If all asteroids have been destroyed create a new batch.
                if(asteroidsLeft <= 0)
                    if(--asteroidsCounter <= 0)
                        initAsteroids();
            }

            // Update the screen and set the timer for the next loop.
            form.Refresh();
            startTime += DELAY;

            while(startTime-DateTime.Now.Ticks/10000 > 0)
                Application.DoEvents();
        }
    }

    public void initShip()
    {
        ship.active = true;
        ship.angle = 0.0;
        ship.deltaAngle = 0.0;
        ship.currentX = 0.0;
        ship.currentY = 0.0;
        ship.deltaX = 0.0;
        ship.deltaY = 0.0;
        ship.render();

        hyperCounter = 0;
    }

    public void updateShip()
    {
        double dx, dy, limit;

        if(!playing)
            return;

        // Rotate the ship if left or right cursor key is down.
        if(left){
            ship.angle += Math.PI / 16.0;

            if(ship.angle > 2 * Math.PI)
                ship.angle -= 2 * Math.PI;
        } 
        else if(right){
            ship.angle -= Math.PI / 16.0;

            if(ship.angle < 0)
                ship.angle += 2 * Math.PI;
        }

        // Fire thrusters if up or down cursor key is down. Don't let ship go past the speed limit.
        dx = -Math.Sin(ship.angle) * mirosi / 30;
        dy =  Math.Cos(ship.angle) * mirosi / 30;

        limit = 0.8 * mirosi;

        if(up){
            if(ship.deltaX+dx>-limit && ship.deltaX+dx<limit)
                ship.deltaX += dx;

            if(ship.deltaY+dy>-limit && ship.deltaY+dy<limit)
                ship.deltaY += dy;
        }
        if(down){
            if(ship.deltaX-dx>-limit && ship.deltaX-dx<limit)
                ship.deltaX -= dx;

            if(ship.deltaY-dy >-limit && ship.deltaY-dy<limit)
                ship.deltaY -= dy;
        }

        // Move the ship. If it is currently in hyperspace, advance the countdown.
        if(ship.active){
            ship.advance();
            ship.render();

            if(hyperCounter > 0)
                hyperCounter--;
        }
        // Ship is exploding, advance the countdown or create a new ship if it is
        // done exploding. The new ship is added as though it were in hyperspace.
        // (This gives the player time to move the ship if it is in imminent danger.)
        // If that was the last ship, end the game.
        else{
            if(--shipCounter <= 0){
                if(shipsLeft > 0){
                    initShip();
                    hyperCounter = HYPER_COUNT;
                } 
                else{
                    endGame();
                }
            }
        }
    }

    public void stopShip()
    {
        ship.active = false;
        shipCounter = SCRAP_COUNT;

        if(shipsLeft > 0)
            shipsLeft--;
    }

    public void initPhotons()
    {
        int i;

        for(i=0; i<MAX_SHOTS; i++){
            photons[i].active = false;
            photonCounter[i] = 0;
        }

        photonIndex = 0;
    }

    public void updatePhotons()
    {
        int i;

        // Move any active photons. Stop it when its counter has expired.
        for(i=0; i<MAX_SHOTS; i++){
            if(photons[i].active){
                photons[i].advance();
                photons[i].render();

                if(--photonCounter[i] < 0)
                    photons[i].active = false;
            }
        }
    }

    public void initUfo()
    {
        // Randomly set flying saucer at left or right edge of the screen.
        ufo.active = true;
        ufo.currentX = -AsteroidsSprite.width / 2;
        ufo.currentY = rnd.NextDouble() * AsteroidsSprite.height;
        ufo.deltaX = mirosp + rnd.NextDouble() * (marosp - mirosp);

        if(rnd.NextDouble() < 0.5){
            ufo.deltaX = -ufo.deltaX;
            ufo.currentX = AsteroidsSprite.width / 2;
        }

        ufo.deltaY = mirosp + rnd.NextDouble() * (marosp - mirosp);

        if(rnd.NextDouble() < 0.5)
            ufo.deltaY = -ufo.deltaY;

        ufo.render();

        // Set counter for this pass.
        ufoCounter = (int) Math.Floor(AsteroidsSprite.width / Math.Abs(ufo.deltaX));
    }

    public void updateUfo()
    {
        int i, d;

        // Move the flying saucer and check for collision with a photon. Stop it when its
        // counter has expired.
        if(ufo.active){
            ufo.advance();
            ufo.render();

            if(--ufoCounter <= 0){
                if(--ufoPassesLeft > 0)
                    initUfo();
                else
                    stopUfo();
            }
            else{
                for(i=0; i<MAX_SHOTS; i++){
                    if(photons[i].active && ufo.isColliding(photons[i])){
                        explode(ufo);
                        stopUfo();
                        score += UFO_POINTS;
                    }
                }
                // On occassion, fire a missle at the ship if the saucer is not
                // too close to it.
                d = (int)Math.Max(Math.Abs(ufo.currentX - ship.currentX), Math.Abs(ufo.currentY - ship.currentY));
                
                if(ship.active && hyperCounter<=0 && ufo.active && !missle.active && d>4*marosi && rnd.NextDouble()<.03)
                    initMissle();
            }
        }
    }

    public void stopUfo()
    {
        ufo.active = false;
        ufoCounter = 0;
        ufoPassesLeft = 0;
    }

    public void initMissle()
    {
        missle.active = true;
        missle.angle = 0.0;
        missle.deltaAngle = 0.0;
        missle.currentX = ufo.currentX;
        missle.currentY = ufo.currentY;
        missle.deltaX = 0.0;
        missle.deltaY = 0.0;
        missle.render();
        missleCounter = 3 * Math.Max(AsteroidsSprite.width, AsteroidsSprite.height) / mirosi;
    }

    public void updateMissle()
    {
        int i;

        // Move the guided missle and check for collision with ship or photon. Stop it when its
        // counter has expired.
        if(missle.active){
            if(--missleCounter <= 0){
                stopMissle();
            }
            else{
                guideMissle();
                missle.advance();
                missle.render();

                for(i=0; i<MAX_SHOTS; i++){
                    if(photons[i].active && missle.isColliding(photons[i])){
                        explode(missle);
                        stopMissle();
                        score += MISSLE_POINTS;
                    }
                }

                if(missle.active && ship.active && hyperCounter<=0 && ship.isColliding(missle)){
                    explode(ship);
                    stopShip();
                    stopUfo();
                    stopMissle();
                }
            }
        }
    }

    public void guideMissle()
    {
        double dx, dy, angle;

        if(!ship.active || hyperCounter>0)
            return;

        // Find the angle needed to hit the ship.
        dx = ship.currentX - missle.currentX;
        dy = ship.currentY - missle.currentY;

        if(dx==0 && dy==0)
            angle = 0;

        if(dx == 0){
            if(dy < 0)
                angle = -Math.PI / 2;
            else
                angle = Math.PI / 2;
        } 
        else{
            angle = Math.Atan(Math.Abs(dy / dx));

            if(dy > 0)
                angle = -angle;
            if(dx < 0)
                angle = Math.PI - angle;
        }

        // Adjust angle for screen coordinates.
        missle.angle = angle - Math.PI / 2;

        // Change the missle's angle so that it points toward the ship.
        missle.deltaX = mirosi / 3 * -Math.Sin(missle.angle);
        missle.deltaY = mirosi / 3 *  Math.Cos(missle.angle);
    }

    public void stopMissle()
    {
        missle.active = false;
        missleCounter = 0;
    }

    public void initAsteroids()
    {
        int i, j;
        int s;
        double theta, r;
        int x, y;

        // Create random shapes, positions and movements for each asteroid.
        for(i=0; i<MAX_ROCKS; i++){
            // Create a jagged shape for the asteroid and give it a random rotation.
            asteroids[i].shape = new Polygon();
            s = MIN_ROCK_SIDES + (int)(rnd.NextDouble() * (MAX_ROCK_SIDES - MIN_ROCK_SIDES));
            
            for(j=0; j<s; j++){
                theta = 2 * Math.PI / s * j;
                r = mirosi + (int) (rnd.NextDouble() * (marosi - mirosi));
                x = (int) -Math.Round(r * Math.Sin(theta));
                y = (int)  Math.Round(r * Math.Cos(theta));
                asteroids[i].shape.addPoint(x, y);
            }

            asteroids[i].active = true;
            asteroids[i].angle = 0.0;
            asteroids[i].deltaAngle = (rnd.NextDouble() - 0.5) / 10;

            // Place the asteroid at one edge of the screen.
            if(rnd.NextDouble() < 0.5){
                asteroids[i].currentX = -AsteroidsSprite.width / 2;

                if(rnd.NextDouble() < 0.5)
                    asteroids[i].currentX = AsteroidsSprite.width / 2;

                asteroids[i].currentY = rnd.NextDouble() * AsteroidsSprite.height;
            } 
            else{
                asteroids[i].currentX = rnd.NextDouble() * AsteroidsSprite.width;
                asteroids[i].currentY = -AsteroidsSprite.height / 2;

                if(rnd.NextDouble() < 0.5)
                    asteroids[i].currentY = AsteroidsSprite.height / 2;
            }

            // Set a random motion for the asteroid.
            asteroids[i].deltaX = rnd.NextDouble() * asteroidsSpeed;

            if(rnd.NextDouble() < 0.5)
                asteroids[i].deltaX = -asteroids[i].deltaX;

            asteroids[i].deltaY = rnd.NextDouble() * asteroidsSpeed;

            if(rnd.NextDouble() < 0.5)
                asteroids[i].deltaY = -asteroids[i].deltaY;

            asteroids[i].render();
            asteroidIsSmall[i] = false;
        }

        asteroidsCounter = STORM_PAUSE;
        asteroidsLeft = MAX_ROCKS;

        if(asteroidsSpeed < marosp)
            asteroidsSpeed++;
    }

    public void initSmallAsteroids(int n)
    {
        int count;
        int i, j;
        int s;
        double tempX, tempY;
        double theta, r;
        int x, y;

        // Create one or two smaller asteroids from a larger one using inactive asteroids. The new
        // asteroids will be placed in the same position as the old one but will have a new, smaller
        // shape and new, randomly generated movements.
        count = 0;
        i = 0;
        tempX = asteroids[n].currentX;
        tempY = asteroids[n].currentY;

        do{
            if(!asteroids[i].active){
                asteroids[i].shape = new Polygon();
                s = MIN_ROCK_SIDES + (int)(rnd.NextDouble() * (MAX_ROCK_SIDES - MIN_ROCK_SIDES));
                
                for(j=0; j<s; j++){
                    theta = 2 * Math.PI / s * j;
                    r = (mirosi + (int) (rnd.NextDouble() * (marosi - mirosi))) / 2;
                    x = (int)-Math.Round(r * Math.Sin(theta));
                    y = (int)Math.Round(r * Math.Cos(theta));
                    asteroids[i].shape.addPoint(x, y);
                }

                asteroids[i].active = true;
                asteroids[i].angle = 0.0;
                asteroids[i].deltaAngle = (rnd.NextDouble() - 0.5) / 10;
                asteroids[i].currentX = tempX;
                asteroids[i].currentY = tempY;
                asteroids[i].deltaX = rnd.NextDouble() * 2 * asteroidsSpeed - asteroidsSpeed;
                asteroids[i].deltaY = rnd.NextDouble() * 2 * asteroidsSpeed - asteroidsSpeed;
                asteroids[i].render();
                asteroidIsSmall[i] = true;
                count++;
                asteroidsLeft++;
            }

            i++;
        }while(i<MAX_ROCKS && count<2);
    }

    public void updateAsteroids()
    {
        int i, j;

        // Move any active asteroids and check for collisions.
        for(i=0; i<MAX_ROCKS; i++){
            if(asteroids[i].active){
                asteroids[i].advance();
                asteroids[i].render();
                
                // If hit by photon, kill asteroid and advance score. If asteroid is large,
                // make some smaller ones to replace it.
                for(j=0; j<MAX_SHOTS; j++){
                    if(photons[j].active && asteroids[i].active && asteroids[i].isColliding(photons[j])){
                        asteroidsLeft--;
                        asteroids[i].active = false;
                        photons[j].active = false;
                        explode(asteroids[i]);
                        
                        if(!asteroidIsSmall[i]){
                            score += BIG_POINTS;
                            initSmallAsteroids(i);
                        } 
                        else{
                            score += SMALL_POINTS;
                        }
                    }
                }

                // If the ship is not in hyperspace, see if it is hit.
                if(ship.active && hyperCounter <= 0 && asteroids[i].active && asteroids[i].isColliding(ship)){
                    explode(ship);
                    stopShip();
                    stopUfo();
                    stopMissle();
                }
            }
        }
    }

    public void initExplosions()
    {
        int i;

        for(i=0; i<MAX_SCRAP; i++){
            explosions[i].shape = new Polygon();
            explosions[i].active = false;
            explosionCounter[i] = 0;
        }

        explosionIndex = 0;
    }

    public void explode(AsteroidsSprite s)
    {
        int c, i, j;

        // Create sprites for explosion animation. The each individual line segment of the given sprite
        // is used to create a new sprite that will move outward  from the sprite's original position
        // with a random rotation.
        s.render();
        c = 2;

        if(detail || s.sprite.npoints<6)
            c = 1;

        for(i=0; i<s.sprite.npoints; i+=c){
            explosionIndex++;

            if(explosionIndex >= MAX_SCRAP)
                explosionIndex = 0;

            explosions[explosionIndex].active = true;
            explosions[explosionIndex].shape = new Polygon();
            explosions[explosionIndex].shape.addPoint(s.shape.xpoints[i], s.shape.ypoints[i]);
            j = i + 1;

            if(j >= s.sprite.npoints)
                j -= s.sprite.npoints;

            explosions[explosionIndex].shape.addPoint(s.shape.xpoints[j], s.shape.ypoints[j]);
            explosions[explosionIndex].angle = s.angle;
            explosions[explosionIndex].deltaAngle = (rnd.NextDouble() * 2 * Math.PI - Math.PI) / 15;
            explosions[explosionIndex].currentX = s.currentX;
            explosions[explosionIndex].currentY = s.currentY;
            explosions[explosionIndex].deltaX = -s.shape.xpoints[i] / 5;
            explosions[explosionIndex].deltaY = -s.shape.ypoints[i] / 5;
            explosionCounter[explosionIndex] = SCRAP_COUNT;
        }
    }

    public void updateExplosions()
    {
        int i;

        // Move any active explosion debris. Stop explosion when its counter has expired.
        for(i=0; i<MAX_SCRAP; i++)
            if(explosions[i].active){
                explosions[i].advance();
                explosions[i].render();

                if(--explosionCounter[i] < 0)
                    explosions[i].active = false;
            }
    }

    public bool checkKeys()
    {
        // Check if any cursor keys have been pressed and set flags.
        left  = key.isDown(keyState.VirtualKeyStates.VK_LEFT);
        right = key.isDown(keyState.VirtualKeyStates.VK_RIGHT);
        up    = key.isDown(keyState.VirtualKeyStates.VK_UP);
        down  = key.isDown(keyState.VirtualKeyStates.VK_DOWN);

        // Spacebar: fire a photon and start its counter.
        if(key.pressed(keyState.VirtualKeyStates.VK_SPACE) && ship.active){
            photonIndex++;

            if(photonIndex >= MAX_SHOTS)
                photonIndex = 0;

            photons[photonIndex].active = true;
            photons[photonIndex].currentX = ship.currentX;
            photons[photonIndex].currentY = ship.currentY;
            photons[photonIndex].deltaX = mirosi * -Math.Sin(ship.angle);
            photons[photonIndex].deltaY = mirosi *  Math.Cos(ship.angle);
            photonCounter[photonIndex] = Math.Min(AsteroidsSprite.width, AsteroidsSprite.height) / mirosi;
        }

        // 'H' key: warp ship into hyperspace by moving to a random location and starting counter.
        if(key.pressed(Keys.H) && ship.active && hyperCounter<=0){
            ship.currentX = rnd.NextDouble() * AsteroidsSprite.width;
            ship.currentX = rnd.NextDouble() * AsteroidsSprite.height;
            hyperCounter = HYPER_COUNT;
        }

        // 'P' key: toggle pause mode and start or stop any active looping sound clips.
        if(key.pressed(Keys.P))
            paused = !paused;

        // 'D' key: toggle graphics detail on or off.
        if(key.pressed(Keys.D))
            detail = !detail;

        //'S' key: start the game, if not already in progress.
        if(key.pressed(Keys.S) && !playing)
            initGame();

        return true;
    }

    public void update()
    {
        int i, c;
        String s;

        // Fill in background and stars.
        form.bmg.FillRectangle(Brushes.Black, 0, 0, fwidth, fheight);

        if(detail)
            for(i=0; i<numStars; i++)
                form.bm.SetPixel(stars[i].X, stars[i].Y, Color.White);

        if(!playing){
            s = "A S T E R O I D S";
            form.bmg.DrawString(s, font, Brushes.White, (fwidth-TextRenderer.MeasureText(s, font).Width)/2, fheight/2);
            s = "QRT 2011";
            form.bmg.DrawString(s, font, Brushes.White, (fwidth-TextRenderer.MeasureText(s, font).Width)/2, fheight/2+fontHeight);

            s = "Game Over";
            form.bmg.DrawString(s, font, Brushes.White, (fwidth-TextRenderer.MeasureText(s, font).Width)/2, fheight/4);
            s = "'S' to Start";
            form.bmg.DrawString(s, font, Brushes.White, (fwidth-TextRenderer.MeasureText(s, font).Width)/2, fheight/4+fontHeight);
        } 
        else if(paused){
            s = "Game Paused";
            form.bmg.DrawString(s, font, Brushes.White, (fwidth-TextRenderer.MeasureText(s, font).Width)/2, fheight/4);
        }

        // Display status and messages.
        form.bmg.DrawString("Score: " + score, font, Brushes.White, fontWidth, fontHeight-8);
        form.bmg.DrawString("Ships: " + shipsLeft, font, Brushes.White, fontWidth, fheight-fontHeight-4);
        s = "High: " + highScore;
        form.bmg.DrawString(s, font, Brushes.White, fwidth-(fontWidth+TextRenderer.MeasureText(s, font).Width), fontHeight-8);

        // Draw photon bullets.
        for(i=0; i<MAX_SHOTS; i++)
            if(photons[i].active)
                form.bmg.DrawPolygon(Pens.White, photons[i].sprite.toArray());

        // Draw the guided missle, counter is used to quickly fade color to black when near expiration.
        if(missle.active){
            c = Math.Min(missleCounter * 24, 255);
            Pen p = new Pen(Color.FromArgb(c, c, c));

            form.bmg.DrawPolygon(p, missle.sprite.toArray());
            form.bmg.DrawLine(p, missle.sprite.xpoints[missle.sprite.npoints-1], missle.sprite.ypoints[missle.sprite.npoints-1],
                              missle.sprite.xpoints[0], missle.sprite.ypoints[0]);
        }

        // Draw the asteroids.
        for(i=0; i<MAX_ROCKS; i++){
            if(asteroids[i].active){
                if(detail)
                    form.bmg.FillPolygon(Brushes.Black, asteroids[i].sprite.toArray());

                form.bmg.DrawPolygon(Pens.White, asteroids[i].sprite.toArray());
                form.bmg.DrawLine(Pens.White, asteroids[i].sprite.xpoints[asteroids[i].sprite.npoints - 1], asteroids[i].sprite.ypoints[asteroids[i].sprite.npoints - 1],
                                  asteroids[i].sprite.xpoints[0], asteroids[i].sprite.ypoints[0]);
            }
        }

        // Draw the flying saucer.
        if(ufo.active){
            if(detail)
                form.bmg.FillPolygon(Brushes.Black, ufo.sprite.toArray());

            form.bmg.DrawPolygon(Pens.White, ufo.sprite.toArray());
            form.bmg.DrawLine(Pens.White, ufo.sprite.xpoints[ufo.sprite.npoints-1], ufo.sprite.ypoints[ufo.sprite.npoints-1],
                              ufo.sprite.xpoints[0], ufo.sprite.ypoints[0]);
        }

        // Draw the ship, counter is used to fade color to white on hyperspace.
        if(ship.active){
            if(detail && hyperCounter == 0)
                form.bmg.FillPolygon(Brushes.Black, ship.sprite.toArray());

            c = 255 - (255 / HYPER_COUNT) * hyperCounter;
            Pen p = new Pen(Color.FromArgb(c, c, c));
            form.bmg.DrawPolygon(p, ship.sprite.toArray());
            form.bmg.DrawLine(p, ship.sprite.xpoints[ship.sprite.npoints - 1], ship.sprite.ypoints[ship.sprite.npoints - 1],
                              ship.sprite.xpoints[0], ship.sprite.ypoints[0]);
        }

        // Draw any explosion debris, counters are used to fade color to black.
        for(i=0; i<MAX_SCRAP; i++){
            if(explosions[i].active){
                c = (255 / SCRAP_COUNT) * explosionCounter[i];
                Pen p = new Pen(Color.FromArgb(c, c, c));
                form.bmg.DrawPolygon(p, explosions[i].sprite.toArray());
            }
        }

        // Copy the off screen buffer to the screen.
        form.formGraphics.DrawImage(form.bm, 0, 0);
    }
}

}