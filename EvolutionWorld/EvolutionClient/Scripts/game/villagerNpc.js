define(['game/unitBase'], function (UnitBase) {
	var VillagerNpc = UnitBase.extend({
		init: function (id) {

			//must be changed in the derived classes
			this._super(id, 'villager');
		},
	})

	return VillagerNpc;
});