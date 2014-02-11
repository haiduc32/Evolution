define(['game/sprite'], function (Sprite) {
	var UnitBase = Class.extend({
		init: function (id, spriteName) {
			//must be changed in the derived classes
			this.spriteName = spriteName;
			this.id = id;
			this.x = 0;
			this.y = 0;
			this.location = {};
			//TODO: figure out what to do with the scale
			this.sprite = new Sprite(this.spriteName, 1);
			this.sprite.setLabel(id);

			var self = this;
			this.sprite.onLoaded(function () {
				//this.showLabel(true);
				self.onLoadedCallback();
			})
		},

		onLoaded: function (callback) {
			this.onLoadedCallback = callback;
		},

		getSprite: function () {
			return this.sprite;
		},

		//not used, delete if not used soon
		//beginMove: function (lastLocation, newLocation) {
		//	this.sprite.beginMove(lastLocation, newLocation);
		//},

		setLocation: function (location) {

			//TODO: this should be a means of ensuring that temporary network problems do not affect the game
			//for now commented out as the code needs rework
			//return;

			this.x = location.X;
			this.y = location.Y;

			this.location = location;

			if (this.sprite != undefined) {
				if (!this.sprite.isOnPath)
					this.sprite.setLocation(location);
			}
		},

		beginPath: function (path) {
			//because of how the Sprite works it won't know it's current location if the current path is 
			//interrupted, so we explicitly set it here
			this.sprite.setLocation(this.location);
			if (this.sprite != undefined) {
				this.sprite.startPath(path);
			}
		},

		followingPath: function (path) {
			if (this.sprite != undefined) {
				if (!this.sprite.isOnPath)
					this.sprite.startPath(path);
			}
		},

		pathFinished: function (location, pathInterrupted) {
			if (pathInterrupted) {
				if (this.sprite != undefined) {
					if (this.sprite.isOnPath) {
						this.sprite.stopPath(location, pathInterrupted);
						//this.sprite.setLocation(location);
					}
				}
			}
		},
	})

	return UnitBase;
});