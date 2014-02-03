var hubUrl = 'http://localhost:9999/signalr/hubs';

requirejs.config({
	//By default load any module IDs from js/lib
	//baseUrl: 'js/lib',
	//except, if the module ID starts with "app",
	//load it from the js/app directory. paths
	//config is relative to the baseUrl, and
	//never includes a ".js" extension since
	//the paths config could be for a directory.
	paths: {
		jquery: 'jquery-2.1.0',
		terminal: 'jquery.terminal-0.7.12',
		kinetic: 'kinetic-v5.0.1.min',
		//kinetic: 'kinetic-v4.4.3',
		signalr: 'jquery.signalR-2.0.1.min',
		"signalr.hubs": 'http://localhost:9999/signalr/hubs?'
	},
	shim: {
		//"jquery": { exports: "$" },
		"signalr": { deps: ["jquery"] },
		"signalr.hubs": { deps: ["signalr"] },
		"terminal": {deps: ["jquery"] },
	}
});

require(['jquery', 'game/gameCanvas', 'game/game', 'class', 'terminal'], function (util, gameCanvas, Game) {
	//This function is called when scripts/helper/util.js is loaded.
	//If util.js calls define(), then this function is not fired until
	//util's dependencies have loaded, and the util argument will hold
	//the module value for "helper/util".

	(function ($) {
		$.fn.tilda = function (eval, options) {
			if ($('body').data('tilda')) {
				return $('body').data('tilda').terminal;
			}
			this.addClass('tilda');
			options = options || {};
			eval = eval || function (command, term) {
				term.echo("you don't set eval for tilda");
			};
			var settings = {
				prompt: 'tilda> ',
				name: 'tilda',
				height: 100,
				enabled: false,
				greetings: 'Quake like console',
				keypress: function (e) {
					if (e.which == 96) {
						return false;
					}
				}
			};
			if (options) {
				$.extend(settings, options);
			}
			this.append('<div class="td"></div>');
			var self = this;
			self.terminal = this.find('.td').terminal(eval,
												   settings);
			var focus = false;
			$(document.documentElement).keypress(function (e) {
				if (e.charCode == 96) {
					self.slideToggle('fast');
					self.terminal.command_line.set('');
					self.terminal.focus(focus = !focus);
				}
			});
			$('body').data('tilda', this);
			this.hide();
			return self;
		};
	})(jQuery);

	$(document).ready(function () {
		$('#tilda').tilda(function (command, terminal) {
			if (command == 'enablelogging') {
				//TODO: show the rminal if it's not shown
			}
			terminal.echo('you type command "' + command + '"');
		});
		$('#terminal').terminal(function (command, term) {
			if (command == 'helo') {
				term.echo('hey there stranger!')
			}
			else  if (command !== '') {
				try {
					var result = window.eval(command);
					if (result !== undefined) {
						term.echo(new String(result));
					}
				} catch (e) {
					term.error(new String(e));
				}
			} else {
				term.echo('');
			}
		}, {
			greetings: 'Evolution console',
			name: 'js_demo',
			height: 200,
			prompt: '> '
		});


		//require(['game/gameCanvas'], function (GameCanvas) {
		//	var gameCanvas = new GameCanvas('canvas2');
		//	gameCanvas.test();
		//});
		//var gameCanvas = gameCanvas;//new GameCanvas('canvas2');

		this.game = new Game(gameCanvas, hubUrl);

		//TODO: move somewhere else
		this.game.start();
	});

});