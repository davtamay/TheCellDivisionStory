mergeInto(LibraryManager.library, {

  InitSocketIOReceive: function(arrayPointer, size){
	console.log('InitSocketIOReceive');
	// keeps track of known entites to reference in network_data heap
	var known_entities = [];
	var entity_index = 0;
	var fields = 14;
    window.socket.on('relayUpdate', function(data) {
		var this_entity_id = data[3];

		// check if we know about this entity yet, if not
		// assign it the last index in the known_entities array
		if(known_entities.indexOf(this_entity_id) === -1) {
			known_entities.push(this_entity_id);
			entity_index = known_entities.length-1;
			console.log('new entity:', known_entities);
		}
		else {
		  entity_index = known_entities.indexOf(this_entity_id);
		}

		// iterate over the data array and repack into the entity slot.
		// NOTE(rob):
		// we use "arrayPointer >> 2" to change the pointer location on the module heap
		// when interpreted as float32 values ("HEAPF32[]"). 
		// for example, an original array value (which is a pointer to a 
		// position on the module heap) of 400 would right shift to 100 
		// which would be the correct corresponding index on the heap
		// for elements of 32-bit size.
		// we use the entity index and the field iterator to index into the 
		// module heap where Unity will read the data back out on Update()
		// we use the same method below to send updates updates to the relay server.

	    for(i = 0; i < data.length; i++){
            HEAPF32[(arrayPointer >> 2) + (entity_index*fields) + i] = data[i];
        }
    });
  },

  InitSocketIOClientCounter: function() {
    window.socket.on('joined', function(client_id){
      window.gameInstance.SendMessage('NetworkManager','RegisterNewClientId', client_id);
	});
  },

  GetClientIdFromBrowser: function() {
      console.log('client_id passed to Unity:', window.client_id);
	  return window.client_id;
  },

  GetSessionIdFromBrowser: function() {
      console.log('session_id passed to Unity:', window.session_id);
	  return window.session_id;
  },
  
  GetIsTeacherFlagFromBrowser: function() {
      console.log('isTeacher passed to Unity:', window.isTeacher);
	  return window.isTeacher;
  },

  SocketIOSendPosition: function (array, size) {
    var buf = [];
    for(var i = 0; i < size; i++)buf.push(HEAPF32[(array >> 2) + i]);
    window.socket.emit("update", buf);
  },

  SocketIOSendInteraction: function (array, size) {
	var buff = []

	for(var i = 0; i < size; i++) {
		buff.push(HEAP32[(array >> 2) + i]);
	}
	window.socket.emit("interact", buff)
  },

  SocketIOSessionJoin: function (joinIds) {
	console.log('SocketIOSessionJoin', joinIds);
	  window.socket.emit("join", joinIds);
  },

  InitSocketIOReceiveInteraction: function(arrayPointer, size) {
	console.log('InitSocketIOReceiveInteraction');
	window.socket.on('interactionUpdate', function(data) {
		for(i = 0; i < data.length; i++){
            HEAP32[(arrayPointer >> 2) + i] = data[i];
        }
	})
  },

  Record_Change: function (operation,session_id) {
    if (operation == 0){
		window.socket.emit("start_recording",session_id);
	} 
	else{
		window.socket.emit("end_recording",session_id);
	}
  },

  GrabAssets:function(){
	  var serializedAsset = JSON.stringify(window.assets);
	  var bufferSize = lengthBytesUTF8(serializedAsset) + 1;
      var buffer = _malloc(bufferSize);
      stringToUTF8(serializedAsset, buffer, bufferSize);

	  return buffer;
  },

  EnableMicrophone: function() {
	enableMicrophone();
  }


});
