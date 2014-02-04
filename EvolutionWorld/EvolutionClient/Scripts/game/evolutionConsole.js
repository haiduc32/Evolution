define(['class'], function () {
	var EvolutionConsole = Class.extend({
		init: function () {
			this.commands = {} ;
		},

		bind: function (command, callback) {
			this.commands[command] = callback;
		},

		execute: function (command) {
			var callback = this.commands[command];

			if (callback === undefined) {
				//TODO: throw some error

				return false;
			}

			//TODO: send the parameters ins
			callback(command);

			return true;
		},
	});

	return new EvolutionConsole();
});