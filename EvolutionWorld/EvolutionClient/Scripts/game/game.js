define(['signalr.hubs', 'game/villagerNpc'], function (hubs, VillagerNpc) {
	var Game = Class.extend({
		init: function (gameCanvas, hubUrl) {
			this.gameCanvas = gameCanvas;

			//Set the hubs URL for the connection
			$.connection.hub.url = hubUrl;

			// Declare a proxy to reference the hub.
			this.evolutionHub = $.connection.evolutionHub;

			this.unitsList = [];

			this.registerEvents();
		},

		registerEvents: function () {
			var self = this;
			var gameCanvas = this.gameCanvas;
			this.evolutionHub.client.loadUnits = function (unitsLocations) {
				$.each(unitsLocations, function (index, data) {
					var villagerNpc = new VillagerNpc(data.id);

					villagerNpc.onLoaded(function () {
						var sprite = villagerNpc.getSprite();
						//gameCanvas.addSprite(sprite);

						villagerNpc.setLocation(data.location);

						self.unitsList[data.id] = villagerNpc;
					});
				});

			};

			//we don't care to separte beginPath and continuePath, because the unit will check if it's on a path
			//already and will ignore the continue path. we need the continue to figure out when the client just
			// connected and untis are on path
			this.evolutionHub.client.unitBeginPath = function (unitId, path) {
				var unit = self.unitsList[unitId];
				if (unit != undefined) unit.followingPath(path);
			};

			this.evolutionHub.client.unitContinuePath = function (unitId, path) {
				var unit = self.unitsList[unitId];
				if (unit != undefined) unit.followingPath(path);
			};

			this.evolutionHub.client.unitEndPath = function (unitId, location, pathInterrupted) {
				var unit = self.unitsList[unitId];
				if (unit != undefined) unit.pathFinished(location, pathInterrupted);
			};

			//this.evolutionHub.client.unitBeginMove = function (unitId, lastLocation, newLocation) {
			//	self.unitsList[unitId].beginMove(lastLocation, newLocation);
			//};

			this.evolutionHub.client.unitEndMove = function (unitId, lastLocation, newLocation) {
				var unit = self.unitsList[unitId];
				if (unit != undefined) unit.setLocation(newLocation);
			}
		},

		start: function () {
			// Start the connection.
			var evolutionHub = this.evolutionHub;
			$.connection.hub.start().done(function () {
				//TODO: remove this logs, inspire from browserquest log system

				$('#log').append('Connected<br />');
				evolutionHub.server.hello().done(function (serverAnswer) {
					$('#log').append("Server said: " + serverAnswer + "<br />");
				});;
			});
		},

		test: function () {

		},
	})

	return Game;
});