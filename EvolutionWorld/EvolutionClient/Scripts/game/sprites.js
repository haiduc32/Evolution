
define(['text!sprites/villager.json'], function () {
    
	var sprites = {};
    
	//skipt the first item (don't know why it's a number..)
	for (i = 0; i < arguments.length; i++) {
		var sprite = JSON.parse(arguments[i]);
		sprites[sprite.id] = sprite;
	}

	//$.each(arguments, function (spriteJson) {
		
	//		var sprite = JSON.parse(spriteJson);
	//		if (sprite.id != undefined) {
	//		sprites[sprite.id] = sprite;
	//	}
    //});
    
    return sprites;
});
