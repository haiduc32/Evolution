define(['kinetic', 'game/evolutionConsole'], function (kineticjs, evolutionConsole) {
	var GameCanvas = Class.extend({
		init: function (containerName) {
			var self = this;

			this.sprites = [];
			this.containerName = containerName;
			this.stage = new Kinetic.Stage({
				container: containerName,
				width: 384,
				height: 384
			});

			this.backgroundLayer = new Kinetic.Layer();
			this.stage.add(this.backgroundLayer);

			this.chessBackground();

			this.entitiesLayer = new Kinetic.Layer();

			this.stage.add(this.entitiesLayer);

			this.animation = new Kinetic.Animation(function (frame) {
				for (i = 0; i < self.sprites.length; i++) {
					self.sprites[i].animate(frame);
				}
			}, this.entitiesLayer);

			evolutionConsole.bind("show-labels", function (parameters) {
				$.each(self.sprites, function (index, value) {
					value.showLabel(true);
				});
			});

			evolutionConsole.bind("hide-labels", function (parameters) {
				$.each(self.sprites, function (index, value) {
					value.showLabel(false);
				});
			});

			this.animation.start();
		},

		addSprite: function (sprite) {
			this.sprites.push(sprite);
			sprite.onSpriteChanged(this.spriteChangedCallback);

			var kineticSprite = sprite.getKineticSprite();
			this.entitiesLayer.add(kineticSprite);
			kineticSprite.start();

			//kineticSprite.stop();
			//kineticSprite.setAnimation('walk_right');
			//kineticSprite.setFrameRate(7); // 10 as an example
			//kineticSprite.start();

			//this.entitiesLayer.draw();
		},

		spriteChangedCallback: function() {
			//TODO: figure this out, what do we need here
		},

		addToCanvas: function (obj) {
			this.entitiesLayer.add(obj);
		},

		chessBackground: function () {
			for (j = 0; j < 16; j++) {
				for (i = 0; i < 16; i++) {
					var rect = new Kinetic.Rect({
						x: i * 24,
						y: j * 24,
						width: 24,
						height: 24,
						fill: (i + j * 16 + (j % 2)) % 2 == 0 ? 'gray' : 'white',
						strokeWidth: 0
					});

					// add the shape to the layer
					this.backgroundLayer.add(rect);
				}
			}
			this.backgroundLayer.draw();


		},

		showLabels: function (parameters) {

		},
	})

	return new GameCanvas('canvas2');
});