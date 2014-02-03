define(['game/sprite'], function (Sprite) {
	var UnitBase = Class.extend({
		init: function (id, spriteName) {
			//must be changed in the derived classes
			this.spriteName = spriteName;
			this.id = id;
			this.x = 0;
			this.y = 0;
			//TODO: figure out what to do with the scale
			this.sprite = new Sprite(this.spriteName, 1);


			var self = this;
			this.sprite.onLoaded(function () {
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

			if (this.sprite != undefined) {
				if (!this.sprite.isOnPath)
					this.sprite.setLocation(location);
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