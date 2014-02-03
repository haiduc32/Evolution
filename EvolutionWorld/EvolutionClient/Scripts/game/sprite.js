define(['kinetic', 'game/sprites', 'game/gameCanvas'], function (kineticjs, sprites, gameCanvas) {
	var Sprite = Class.extend({
		init: function (typeName, scale) {
			this.x = 0;
			this.y = 0;
			this.typeName = typeName;
			this.scale = scale;
			this.animationState = 'idle_down';
			this.dirrection = 'down';
			this.isLoaded = false;

			this.isOnPath = false;
			this.pathLeg = 0;

			this.cellSize = 24;

			this.label = '';

			this.clearAnimation();


			this.loadJSON(sprites[typeName]);
		},

		setLabel: function (label) {
			this.label = label;
		},

		showLabel: function(show) {
			this.isShowLabel = show;
			if (show) {
				var tooltip = new Kinetic.Label({
					x: 170,
					y: 75,
					opacity: 0.75
				});

				tooltip.add(new Kinetic.Tag({
					fill: 'black',
					pointerDirection: 'down',
					pointerWidth: 10,
					pointerHeight: 10,
					lineJoin: 'round',
					shadowColor: 'black',
					shadowBlur: 10,
					shadowOffset: { x: 10, y: 20 },
					shadowOpacity: 0.5
				}));

				tooltip.add(new Kinetic.Text({
					text: this.label,
					fontFamily: 'Calibri',
					fontSize: 13,
					padding: 5,
					fill: 'white'
				}));
				
				this.kineticLabel = tooltip;

				//var simpleText = new Kinetic.Text({
				//	x: this.x,
				//	y: this.y,
				//	text: 'Simple Text',
				//	fontSize: 14,
				//	fontFamily: 'Calibri',
				//	fill: 'black'
				//});
				//this.kineticLabel = simpleText;

				gameCanvas.addToCanvas(tooltip);
			}
		},

		loadJSON: function (data) {
			this.dataId = data.id;
			this.filename = "img/" + this.scale + "/" + this.dataId + ".png";

			this.animationData = data.animations;
			this.width = data.width;
			this.height = data.height;
			this.offsetX = (data.offset_x !== undefined) ? data.offset_x : -16;
			this.offsetY = (data.offset_y !== undefined) ? data.offset_y : -16;

			this.transformKineticAnimations();

			this.load();


		},

		load: function () {
			var self = this;

			this.image = new Image();
			this.image.src = this.filename;

			this.image.onload = function () {
				self.isLoaded = true;

				self.kineticSprite = new Kinetic.Sprite({
					x: self.offsetX / 2,
					y: self.offsetY / 2,
					image: self.image,
					animation: self.animationState,
					animations: self.kineticAnimation,
					frameRate: 2,
					frameIndex: 0
				});

				gameCanvas.addSprite(self);
				
				if (self.spriteLoadedCallback) {
					self.spriteLoadedCallback();
				}
			};
		},

		transformKineticAnimations: function () {
			var self = this;

			function transformAnimArray(animationType) {
				var animArray = new Array();
				for (i = 0; i < animationType.length; i++) {

					animArray[i * 4] = i * self.width; //x
					animArray[i * 4 + 1] = animationType.row * self.height; //y
					animArray[i * 4 + 2] = self.width; //width
					animArray[i * 4 + 3] = self.height; //height
				}

				return animArray;
			}

			this.kineticAnimation = {
				atk_right: transformAnimArray(this.animationData.atk_right),
				walk_right: transformAnimArray(this.animationData.walk_right),
				idle_right: transformAnimArray(this.animationData.idle_right),
				atk_up: transformAnimArray(this.animationData.atk_up),
				walk_up: transformAnimArray(this.animationData.walk_up),
				idle_up: transformAnimArray(this.animationData.idle_up),
				atk_down: transformAnimArray(this.animationData.atk_down),
				walk_down: transformAnimArray(this.animationData.walk_down),
				idle_down: transformAnimArray(this.animationData.idle_down),
				atk_left: transformAnimArray(this.animationData.atk_left),
				walk_left: transformAnimArray(this.animationData.walk_left),
				idle_left: transformAnimArray(this.animationData.idle_left)
			};

		},

		//registers for the KineticSprite change (the object is changed with another and 
		//must be re-registered with the gameCanvas).
		onSpriteChanged: function (callback) {
			this.spriteChangedCallback = callback;
		},

		getKineticSprite: function () {
			return this.kineticSprite;
		},

		onLoaded: function (callback) {
			this.spriteLoadedCallback = callback;
		},

		
		setLocation: function (location) {
			if (this.kineticSprite != undefined && !this.isOnPath) {
				this.lastLocation = location;

				var x = location.X * this.cellSize;// + this.offsetX / 2;
				var y = location.Y * this.cellSize;// + this.offsetY / 2;
				this.setPosition(x, y);
			}
		},

		setPosition: function (x, y) {
			/// <summary>Sets the sprite position on it's layer.</summary>
			/// <param name="x" type="Number">The x position on the layer (in pixels).</param>
			/// <param name="y" type="Number">The y position on the layer (in pixels).</param>
			var relX = x + this.offsetX / 2;
			var relY = y + this.offsetY / 2;
			this.kineticSprite.position({ x: relX, y: relY });
			if (this.showLabel) {
				this.kineticLabel.position({ x: relX +16, y: relY });
			}
		},

		startPath: function (path) {
			//TODO: implement

			this.path = path;
			this.pathLeg = 0;
			this.isOnPath = true;

			this.beginMove(this.lastLocation, this.path[this.pathLeg]);
		},

		stopPath: function (location) {
			this.clearAnimation();
			this.isOnPath = false;

			this.setIdle();

			this.setLocation(location);
		},

		beginMove: function (lastLocation, newLocation) {
			this.lastLocation = lastLocation;
			this.newLocation = newLocation;
			this.animationType = 'move';
			this.animationLength = 500;
			this.animationLeft = 500;
			this.animationInProgress = true;
		},

		setIdle: function() {
			this.kineticSprite.stop();
			this.kineticSprite.setFrameRate(2);
			this.kineticSprite.setAnimation('idle_down');
			this.kineticSprite.start();
		},

		clearAnimation: function () {
			//animation variables
			this.animationInProgress = false;
			this.animationType = '';
			this.animationLength = 0;
			this.animationLeft = 0;
		},

		animate: function (frame) {
			//TODO: implement
			if (this.animationInProgress) {
				if (this.animationType == 'move') {
					var walkingAnimation = this.getWalkingAnimation(this.lastLocation, this.newLocation);

					if (this.animationLength == this.animationLeft) {
						if (this.pathLeg == 0) {
							this.walkingAnimation = walkingAnimation;
							this.kineticSprite.stop();
							this.kineticSprite.setAnimation(walkingAnimation);
							this.kineticSprite.setFrameRate(10);
							this.kineticSprite.start();
						}
						else {
							//check if the sprite needs to change
							//account for the dirrection change
							if (this.walkingAnimation != walkingAnimation) {
								this.walkingAnimation = walkingAnimation;
								this.kineticSprite.stop();
								this.kineticSprite.setAnimation(walkingAnimation);
								this.kineticSprite.setFrameRate(10);
								this.kineticSprite.start();
							}
						}

					}

					this.animationLeft -= frame.timeDiff;
					if (this.animationLeft < 0) this.animationLeft = 0;

					var animationProgress = (this.animationLength - this.animationLeft) / this.animationLength;

					var deltaX = Math.floor((this.newLocation.X - this.lastLocation.X) * this.cellSize * animationProgress);
					var deltaY = Math.floor((this.newLocation.Y - this.lastLocation.Y) * this.cellSize * animationProgress);

					this.setPosition(this.lastLocation.X * this.cellSize + deltaX, this.lastLocation.Y * this.cellSize + deltaY);



					if (this.animationLeft == 0) {
						//at the end of the animation setup the lastLocation
						this.lastLocation = this.newLocation;

						this.pathLeg += 1;

						//if there are no more legs, idle
						if (this.pathLeg == this.path.length) {
							this.isOnPath = false;
							this.setIdle();
							this.clearAnimation();
						}
						else {
							//move to the next leg
							this.beginMove(this.lastLocation, this.path[this.pathLeg])
						}
					}
				}
			}
		},

		getWalkingAnimation: function (lastLocation, newLocation) {
			if (lastLocation.X == newLocation.X && lastLocation.Y > newLocation.Y) {
				return "walk_up";
			} else if (lastLocation.X == newLocation.X && lastLocation.Y < newLocation.Y) {
				return "walk_down";
			} else if (lastLocation.X > newLocation.X && lastLocation.Y == newLocation.Y) {
				return "walk_left";
			} else if (lastLocation.X < newLocation.X && lastLocation.Y == newLocation.Y) {
				return "walk_right";
			}
		},
	})

	return Sprite;
});