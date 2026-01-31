// Copyright (C) 2020-2022 AvantLogic Corporation

class MapEditor
{
	constructor(map)
	{
		console.log(`MapEditor::constructor`);

		this.map = map;
		this.page = map.page;
		this.isGalleryEditor = map.page.isGallery;
		this.isMapEditor = !map.page.isGallery;

		// Provide quick access to the editor.
		window.MapEditor = this;

		// Initially no markers are selected or being edited.
		this.selectedMarkers = new SelectedMarkers(this);
		this.markerBeingEdited_ = null;
		this.hybridEditingEnabled = false;
		this.hybridMarkerBeingEdited = null;
		this.lastShape = "rectangle";
		this.unsavedMarkerEdits = [];
		this.otherInstancesOfMarkerBeingEdited = [];
		this.clearEditHistory();
		this.excludeEditFromHistory = false;

		this.initializeActions();

		// Set the controls to their state when no marker is selected.
		this.initializeControls();

		this.viewIdForMarkerUnderPointer = 0;
		this.viewIdForMarkerBeingDragged = 0;
		this.viewIdForMarkerWithCrosshairs = 0;

		this.currentPointerLocation = null;
		this.lastDragLocation = null;
		this.draggingMarker = false;
		this.dragDeltaX = 0;
		this.dragDeltaY = 0;

		this.draggingCrosshairs = false;
		this.crosshairRectUnderPointer = null;

		this.draggingHandle = false;
		this.dragHandleDistanceX = 0;
		this.dragHandleDistanceY = 0;
		this.ignoreClick = false;

		this.eraseCrossHairRects();
		this.inMapFocusingState = false;
		this.settingFlexLock = false;
		this.showingNotificationMessage = false;

		this.animatedMarker = null;

		this.cancelInstructions = '';

		// Bind event handlers so that when they are called, 'this' will be set to this MapsAliveMap object.
		this.performDragOnEachAnimationFrame = this.performDragOnEachAnimationFrame.bind(this);
		this.placeMarkerOnMapAnimation = this.placeMarkerOnMapAnimation.bind(this);
	}

	actionAddNewHotspot()
	{
		if (this.featureDisabled("Add Hotspot"))
			return;

		else if (hotspotLimitReached)
			maAlert("[You have used all of your hotspots. To purchase more:][Choose [@Account > Buy More Hotspots@] from the menu.]")
		else
			this.showDialogAddNewHotspot();
	}

	actionAlign(direction)
	{
		if (this.editingMarker && this.markerBeingEdited.shapeHasPoints)
			this.alignHandles(direction);
		else
			this.alignMarkers(direction);
	}

	actionAllowOverZoom()
	{
		if (this.featureDisabled("Over-zoom"))
			return;

		this.allowOverZoomEnabled = !this.allowOverZoomEnabled;
		this.activateControl(this.ACTION_ALLOW_OVER_ZOOM, this.allowOverZoomEnabled);

		this.map.allowOverZoom = this.allowOverZoomEnabled;

		// When the user disables over-zoom and the map is over-zoomed, zoom it back to 100%;
		if (!this.allowOverZoomEnabled && this.map.currentMapScale > 1.0)
			this.map.zoomMap(-1);

		this.map.drawMap();
	}

	async actionCancelEditing()
	{
		if (this.editHistory.length > 1)
		{
			// Display a confirm dialog but don't allow the to close it since Esc can cause this method to be called.
			const confirm = await maAwaitConfirm("Discard changes?", 'DISCARD', false);
			if (confirm === false)
				return;
		}

		this.restoreMarkerEdit(this.markerBeingEdited, this.editHistory[0]);
		this.clearEditHistory()
		this.finishEditingMarker();
		this.redrawSelectedMarkers();
		this.updateControls();
	}

	actionChangeStackOrder()
	{
		if (this.featureDisabled("Stack Ordering"))
			return;

		this.showDialogChangeStackOrder();
	}

	async actionConvertHybrid()
	{
		if (this.featureDisabled("Convert Marker"))
			return;

		// When the action is for a hybrid marker that is being edited, disable hybrid editing
		// which actually converts the hybrids pseudo markers back into a hybrid marker.
		if (this.hybridEditingEnabled)
		{
			this.enableHybridEditing(false);
			return;
		}

		// Determine what kind of conversion to perform.
		let marker = this.selectedMarkers.primaryMarker;
		let message;
		if (marker.shapeIsHybrid)
		{
			// Determine how many parts the shape has.
			let shapes = marker.shapeCoords.split("-1,");
			let parts = shapes.length - 1;
			if (parts > 1)
			{
				maAlert(`You can only convert a hybrid marker back to a shape marker when the hybrid has only 1 shape, but this hybrid marker has ${parts} shapes.`);
				return;
			}
			message = "Convert the hybrid marker back to a shape marker?";
		}
		else
		{
			message = "Convert the shape marker to a hybrid marker?";
		}

		// Ask the user to confirm that they want to convert a shape to a hybrid or vice versa.
		const confirm = await maAwaitConfirm(message, 'CONVERT');
		if (confirm === false)
			return;

		// Perform the conversion.
		this.postConvertHybrid(marker);
	}

	actionCreateNewMarker()
	{
		if (this.featureDisabled("Create Marker"))
			return;

		if (this.hybridEditingEnabled)
		{
			this.showDialogCreateNewPseudoMarker();
		}
		else
		{
			const duplicate = false;
			this.showDialogCreateNewMarker(duplicate);
		}
	}

	actionDuplicateMarker()
	{
		if (this.featureDisabled("Duplicate Marker"))
			return;

		const duplicate = true;
		this.showDialogCreateNewMarker(duplicate);
	}

	async actionEditMarker()
	{
		if (this.featureDisabled("Edit Marker"))
			return;

		if (this.selectedMarkers.primaryMarker.rotationDegrees !== 0)
		{
			maAlert("You cannot edit a rotated marker. You can rotate it back to 0 degrees, edit it, and then rotate it again.");
			return;
		}

		let marker = this.selectedMarkers.primaryMarker;

		if (marker.shapeIsHybrid)
		{
			this.enableHybridEditing(true);
			return;
		}

		if (marker.typeIsPhoto)
		{
			maAlert("You cannot edit a photo marker.");
			return;
		}

		this.otherInstancesOfMarkerBeingEdited = [];

		// Count the number of other hotspots on this map that use the marker.
		let count = -1;
		for (const hotspot of hotspotData)
		{
			if (hotspot.markerId !== marker.markerId)
				continue;

			count += 1;

			let affectedMarker = this.map.getMarker(hotspot.viewId);
			if (affectedMarker !== marker)
				this.otherInstancesOfMarkerBeingEdited.push(affectedMarker);
		}

		// Warn the user if one or more other hotspots use the marker.
		let okToEdit = true;
		if (count >= 1)
		{
			let plural = count >= 2 ? 's' : '';
			let message = `[*${count} other hotspot${plural} on this map use this marker.*]`;
			message += "[Your edits will affect all uses of the marker on all maps.]";
			okToEdit = await maAwaitConfirm(message, 'EDIT');
		}

		if (okToEdit)
			this.beginEditingMarker(marker);
	}

	actionFinishEditing()
	{
		// Apply the edits to the other instances of the same marker.
		for (const marker of this.otherInstancesOfMarkerBeingEdited)
		{
			marker.shapeW = this.markerBeingEdited.shapeW;
			marker.shapeH = this.markerBeingEdited.shapeH;
			marker.shapeCoords = this.markerBeingEdited.shapeCoords;
			marker.updateBounds();
		}
		this.otherInstancesOfMarkerBeingEdited = [];

		this.finishEditingMarker();

		this.redrawSelectedMarkers();
		this.updateControls();
	}

	actionGoToEditScreen()
	{
		this.showDialogGoToEditScreen();
	}

	actionNudge(distance, direction)
	{
		if (!this.ctrlKeyIsDown)
			distance *= 5;

		// Scale the disance so that a 1 pixel nudge moves 1 pixel on the screen.
		let scaledDistance = this.map.convertScreenToMap(distance);

		// Make sure the distance is not zero which can happen if the map is over-zoomed and the
		// ctrl key is down such that a value of 1 or -1 scaled to the map and rounded is zero.
		if (Math.abs(scaledDistance) === 0)
			scaledDistance = distance;

		// If this is the first nudge action following another kind of action, flush the marker layer
		// cache so that moveMarkers can optimize subsequent nudges by resusing the cache during nudging.
		if (!this.nudging)
			this.map.flushMarkerLayerCache();

		if (this.okToNudgeHandle)
			this.nudgeHandle(scaledDistance, direction);
		else
			this.nudgeMarkers(scaledDistance, direction);
	}

	actionRedoEdit()
	{
		// Ignore the request if there's nothing to redo. This can happen if the users presses ctrl-y.
		if (!this.allowRedo)
			return;

		this.editHistoryIndex += 1;

		let editToRestore = this.editHistory[this.editHistoryIndex];

		this.restoreMarkerEdit(this.markerBeingEdited, editToRestore);
		this.updateControls();
	}

	actionRemove()
	{
		if (this.editingMarker)
		{
			if (this.markerBeingEdited.handles.selectedHandle !== null)
				this.removeHandle();
		}
		else
		{
			if (this.selectedMarkers.count >= 1)
			{
				if (this.hybridEditingEnabled)
				{
					this.removePseudoMarkerFromHybrid();
				}
				else
				{
					if (this.ctrlKeyIsDown)
						this.deleteHotspot();
					else
						this.removeMarker();
				}
			}
		}
	}

	actionReplaceMarker()
	{
		if (this.featureDisabled("Replace Marker"))
			return;

		this.showDialogReplaceMarker();
	}

	actionReplaceMarkerStyle()
	{
		if (this.featureDisabled("Replace Marker Style"))
			return;

		this.showDialogReplaceMarkerStyle();
	}

	actionRotate(degrees)
	{
		if (!this.ctrlKeyIsDown)
			degrees *= 5;

		//console.log(`MapEditor::actionRotate ${degrees} degrees`);

		if (!this.rotating)
		{
			this.map.flushMarkerLayerCache();
		}

		this.eraseEditLayer();

		for (const viewId of this.selectedMarkers.viewIds)
		{
			let marker = this.map.getMarker(viewId);

			if (marker.isLocked)
				continue;

			marker.rotationDegrees += degrees;

			let delta = Math.abs(marker.rotationDegrees % 180);
			if (marker.rotationDegrees > 180)
				marker.rotationDegrees = (180 - delta) * -1;
			else if (marker.rotationDegrees < -179)
				marker.rotationDegrees = 180 - delta;

			marker.rotationRadians = marker.rotationDegrees * Math.PI / 180;
			marker.updateBounds();

			this.showMarkerCoordsMessage(marker);
		}

		// Draw the unselected markers.
		if (this.map.markerLayerCacheIsStale)
			this.drawAndCacheUnselectedMarkers();
		else
			this.map.drawCachedMarkerLayer();

		//// Draw the markers that are being rotated.
		this.redrawSelectedMarkers();

		if (!this.hybridEditingEnabled)
			this.map.reportMarkerCoords();

		this.updateControls();
	}

	actionSelectAllMarkers()
	{
		// Using the Select All Markers control with the Ctrl key deselects any selected markers.
		if (this.ctrlKeyIsDown)
		{
			this.deselectAllMarkers();
			return;
		}

		// Remove any selected marker from the selected markers list instead figuring out
		// which markers need to be added. Then add all of the visible markers to the list.
		this.selectedMarkers.removeAllMarkers();
		for (const marker of this.map.markers)
		{
			if (marker.isNotOnMap)
				continue;
			this.selectedMarkers.addMarker(marker.viewId);
			maShowMarkerThumbAsSelected(marker.viewId);
		}

		this.redrawSelectedMarkers();

		// When only 1 marker is selected, show it in the hotspot list, otherwise don't show any.
		let viewId = this.selectedMarkers.count === 1 ? this.selectedMarkers.primaryViewId : 0;
		maShowMarkerInHotspotList(viewId);

		this.updateControls();
		this.setMarkerLockImage();
	}

	async actionSetMapFocus()
	{
		// Check if the user is holding the control key to indicate that they want to set the focus point.
		let setFocusPoint = this.ctrlKeyIsDown;

		await this.recordMapState({ mapFocus: true, setFocusPoint: setFocusPoint });
	}

	actionShowMapFocus()
	{
		this.deselectAllMarkers();
		this.eraseEditLayer();

		let updatePosition = false;

		if (Math.round(this.map.currentMapScale * 100) !== this.map.mapFocusPercent)
		{
			// When the zoom level is not the same, update the position.
			updatePosition = true;
		}
		else
		{
			// Calculate the screen coordinates for the map's focus point to determine if the
			// point if visible.
			let { x, y } = this.calculateFocusPoint();
			updatePosition = x > this.map.mapAreaW || x < 0 || y > this.map.mapAreaH || y < 0;
		}

		// Update the position if either the zoom level is different or the point is not visible.
		if (updatePosition)
			this.map.positionAndZoomMapToInitialFocus();

		this.map.drawMap();
		this.map.mapZoomPanStateChanged();

		// Calculate the screen coordinates for the map's focus point again in case the map position changed.
		let { x, y } = this.calculateFocusPoint();

		// Draw the crosshairs through the focus point unless it's at 0,0 as when zoomed all the way out.
		if (this.map.mapFocusX > 0 && this.map.mapFocusY > 0)
			this.drawCrosshairs(this.map.editLayerContext, x, y, '#306EFF');

		this.showFocusStateInformation();
	}

	actionToggleAppearance()
	{
		this.toggleAppearanceEnabled = !this.toggleAppearanceEnabled;
		this.activateControl(this.ACTION_TOGGLE_APPEARANCE, this.toggleAppearanceEnabled);

		this.map.setMarkerAppearanceToggled(this.toggleAppearanceEnabled);
		this.map.drawMap();
		this.redrawSelectedMarkers();
	}

	actionToggleLockMarker()
	{
		let locked = this.selectedMarkersAreLocked();
		for (const viewId of this.selectedMarkers.viewIds)
		{
			let marker = this.map.getMarker(viewId);
			marker.isLocked = !locked;
		}

		// Erase and redraw the outline and crosshairs. The erase is necessary to prevent drawing the color
		// and line dash style for a locked marker on top of the lines for an unlocked marker, and vice versa.
		this.eraseEditLayer();
		this.drawOutlineForMarker(this.selectedMarkers.primaryMarker);

		this.setMarkerLockImage();

		// Report the coords because they indicate whether the marker is locked.
		this.map.reportMarkerCoords();
	}

	actionToggleSoftControlKey()
	{
		this.toggleSoftControlKeyEnabled = !this.toggleSoftControlKeyEnabled;
		this.activateControl(this.ACTION_TOGGLE_SOFT_CONTROL_KEY, this.toggleSoftControlKeyEnabled);
	}

	actionToggleTransparency()
	{
		this.toggleTransparencyEnabled = !this.toggleTransparencyEnabled;
		this.activateControl(this.ACTION_TOGGLE_TRANSPARENCY, this.toggleTransparencyEnabled);

		this.map.setMarkerTransparencyToggled(this.toggleTransparencyEnabled);
		this.map.drawMap();
		this.redrawSelectedMarkers();
	}

	actionUndoEdit()
	{
		// Ignore the request if there's nothing to undo. This can happen if the users presses ctrl-z.
		if (!this.allowUndo)
			return;

		// Index the most recent edit on the stack.
		this.editHistoryIndex -= 1;

		let	editToRestore = this.editHistory[this.editHistoryIndex];

		this.restoreMarkerEdit(this.markerBeingEdited, editToRestore);
		this.updateControls();
	}

	activateControl(actionId, activate)
	{
		this.getControl(actionId).activate(activate);
	}

	addEditToHistory(edit)
	{
		// When current action involves a multi-step operation where some of the interim steps should
		// not be added to the history, ignore the request. This happens when deleting a handle and
		// when nudging multiple handles.
		if (this.excludeEditFromHistory)
			return;

		// Determine if the edit is the same as the previous edit, and if so, ignore it. This also 
		// happens when deleting a handle, but some of the steps occur outside ignoreEdit bracketing.
		// some of the steps. This logic is necessary to prevent the undo or redo of a handle delete
		// from requiring the user to undo/redo twice because of the intermediate steps.
		if (this.editHistory.length > 0)
		{
			let lastEdit = this.editHistory[this.editHistory.length - 1];
			let same = JSON.stringify(edit) === JSON.stringify(lastEdit);
			if (same)
				return;
		}

		// Remove any history that is older than the last redo. This honors the rule that if
		// you start making edits after a redo, any edits prior to the redo cannot be undone.
		this.editHistory.splice(this.editHistoryIndex + 1);

		this.editHistory.push(edit);
		this.editHistoryIndex = this.editHistory.length - 1;
	}

	addPseudoMarkerToHybrid(newShape)
	{
		const hybridMarker = this.hybridMarkerBeingEdited;
		const shapeType = this.shapeType(this.hybridMarkerBeingEdited, newShape);

		// Get the center of the hybrid shape.
		let x = Math.round(hybridMarker.shapeW / 2);
		let y = Math.round(hybridMarker.shapeH / 2);
		let deltaX;
		let deltaY;

		// Create default coords for the new shape. The defaults are calculated server-side based
		// on the size of the map image. The same defaults are used when creating a new shape marker.
		let coords = "";
		switch (shapeType)
		{
			case hybridMarker.SHAPE_TYPE_CIRCLE:
				// Use the hybrid center as the circle's center.
				coords = hybridMarker.shapeCoordsToShapeCoordsArray(defaultCoords.circle);
				coords[0] = x;
				coords[1] = y;
				break;

			case hybridMarker.SHAPE_TYPE_RECTANGLE:
				// Get the coords for the rectangle's upper left and lower right corners.
				coords = hybridMarker.shapeCoordsToShapeCoordsArray(defaultCoords.rectangle);

				// Shift the corners so that the rectangle will be centered in the hybrid.
				let w = coords[2] - coords[0] + 1;
				let h = coords[3] - coords[1] + 1;
				deltaX = Math.round((hybridMarker.shapeW - w) / 2);
				deltaY = Math.round((hybridMarker.shapeH - h) / 2);
				coords[0] += deltaX;
				coords[1] += deltaY;
				coords[2] += deltaX;
				coords[3] += deltaY;
				break;

			case hybridMarker.SHAPE_TYPE_POLYGON:
			case hybridMarker.SHAPE_TYPE_LINE:
				// Get the default coords.
				if (shapeType === hybridMarker.SHAPE_TYPE_POLYGON)
					coords = hybridMarker.shapeCoordsToShapeCoordsArray(defaultCoords.polygon);
				else
					coords = hybridMarker.shapeCoordsToShapeCoordsArray(defaultCoords.line);

				// Determine the size of the shape's bounding rectangle
				let boundingInfo = this.calculatePolygonBoundingRectangle(coords);

				// Shift each point so that the shape will be centered in the hybrid.
				deltaX = Math.round((hybridMarker.shapeW - boundingInfo.size.w) / 2);
				deltaY = Math.round((hybridMarker.shapeH - boundingInfo.size.h) / 2);
				for (let i = 0; i < coords.length; i += 2)
				{
					coords[i] += deltaX;
					coords[i + 1] += deltaY;
				}
				break;
		}

		coords = `,-1,${shapeType},${coords.join(",")}`;

		// Disable hybrid editing and add the new coords to the hybrid. Then reenable hybrid editing
		// and recreate the hybrid marker's pseudo markers which will now include the newly added shape.
		this.enableHybridEditing(false);
		hybridMarker.shapeCoords += coords;
		this.enableHybridEditing(true);
		this.map.createHybridMarkerFromPseudoMarkers(hybridMarker);
		this.reportMarkerEdit(this.hybridMarkerBeingEdited);

		this.eraseEditLayer();
		this.map.drawMap();

		// Set the newly added shape as the selected pseudo marker.
		const newPseudoMarker = this.map.markers[this.map.markers.length - 1];
		this.setMarkerSelected(newPseudoMarker.viewId);
	}

	alignHandles(direction)
	{
		const marker = this.markerBeingEdited;
		let coords = marker.shapeCoordsArray;

		let first = marker.handles.selectedHandleIndex * 2;

		for (const index of marker.handles.selected)
		{
			if (index === marker.handles.selectedHandleIndex)
				continue;

			let i = index * 2;

			if (direction === 'v')
				coords[i] = coords[first];
			else if (direction === 'h')
				coords[i + 1] = coords[first + 1];
		}

		marker.shapeCoordsArray = coords;

		this.updateMarkerAndMapAfterEdit(marker);
	}

	alignMarkers(direction)
	{
		// Align all the markers with the primary marker.
		let primaryMarker = this.selectedMarkers.primaryMarker;

		let markerPositions = [];

		for (const viewId of this.selectedMarkers.viewIds) 
		{
			if (viewId === primaryMarker.viewId)
				continue;

			let marker = this.map.getMarker(viewId);

			if (marker.isLocked)
				continue;

			let x;
			let y;

			if (direction === 'h')
			{
				x = marker.x;
				y = primaryMarker.y;
			}
			else
			{
				x = primaryMarker.x;
				y = marker.y;
			}

			markerPositions.push(this.createMarkerPosition(marker, x, y));
		}

		this.map.flushMarkerLayerCache();
		this.moveMarkers(markerPositions);
	}

	get allowRedo()
	{
		return this.editingMarker && this.editHistoryIndex >= 0 && this.editHistoryIndex < this.editHistory.length - 1;
	}

	get allowUndo()
	{
		return this.editingMarker && this.editHistoryIndex > 0;
	}

	get animatingMarker()
	{
		return this.animatedMarker !== null;
	}

	applyEditToHandle(deltaX, deltaY, index = -1, pointer = null)
	{
		// This method updates a marker's shape following an edit to one of its handles. The
		// edit can be a nudge, drag, removal, on conversion of a breakpoint to a handle.

		// The deltaX and deltaY values indicate how the marker's shape changed.
		// A positive deltaX means a point moved right and a negative deltaX means a point moved left. 
		// A positive deltaY means a point moved down and a negative deltaY means a point moved up.
		// Depending on which point moved, the change could make the bounding rectangle wider or narrower,
		// or neither if the point was within and stayed within a polygon's original bounding rectangle.

		let marker = this.markerBeingEdited;
		//if (pointer) console.log(`MapEdtor::applyEditToShapeHandle ${deltaX},${deltaY} : ${pointer.x},${pointer.y}`);

		switch (marker.shapeType)
		{
			case marker.SHAPE_TYPE_CIRCLE:
				this.applyEditToHandleForCircle(marker, deltaX, deltaY, index, pointer);
				break;

			case marker.SHAPE_TYPE_RECTANGLE:
				this.applyEditToHandleForRectangle(marker, deltaX, deltaY, index, pointer);
				break;

			case marker.SHAPE_TYPE_POLYGON:
			case marker.SHAPE_TYPE_LINE:
				this.applyEditToHandleForPolygon(marker, deltaX, deltaY, index, pointer);
				break;

			case marker.SHAPE_TYPE_HYBRID:
				break;
		}

		this.updateMarkerAndMapAfterEdit(marker);
	}

	applyEditToHandleForCircle(marker, deltaX, deltaY, index, pointer)
	{
		// The four handles of a circle going clockwise from the 9 o'clock have indices: 0, 1, 2, 3.
		let deltaRadius;

		if (pointer)
		{
			// When a pointer is passed, the edit is for dragging a handle. The pointer is the
			// mouse location in screen pixels. Convert it to a map location and shift it to
			// adjust for the rectangle's location on the screen and the handle being dragged.
			let radius = Math.round(marker.shapeW / 2);
			let delta = this.getHandleDragDelta(marker, pointer);
			deltaX = delta.x;
			deltaY = delta.y;

			//if (pointer) console.log(`MapEdtor::applyEditToHandleForCircle ${deltaX},${deltaY} : ${pointer.x},${pointer.y}`);

			switch (index)
			{
				case 0:
					deltaY -= radius;
					break;

				case 1:
					deltaX -= marker.shapeW;
					break;

				case 2:
					deltaX -= marker.shapeW;
					deltaY -= radius;
					break;

				case 3:
					deltaX -= radius;
					deltaY -= marker.shapeH;
					break;
			}
		}
		else
		{
			// Allow nudging in any direction.
			if (deltaX === 0)
				deltaX = deltaY;
			else if (deltaY === 0)
				deltaY = deltaX;
		}
		
		if (index === 0)
			deltaRadius = deltaX *= -1;
		else if (index === 1)
			deltaRadius = deltaY *= -1;
		else if (index === 2)
			deltaRadius = deltaX;
		else if (index === 3)
			deltaRadius = deltaY;

		let width = marker.shapeW + deltaRadius * 2;
		if (width < 8)
			width = 8;

		// Update the circle's size and coords.
		marker.shapeW = width;
		marker.shapeH = width;
		const radius = Math.round(width / 2);
		marker.shapeCoords = `0,0,${radius},0`;
	}

	applyEditToHandleForPolygon(marker, deltaX, deltaY, index, pointer)
	{
		let coords = marker.shapeCoordsArray;

		// Update the polygon's coordinates for the handle that moved.
		if (!(deltaX === 0 && deltaY === 0))
		{
			// And index value of -1 means use the selected handle.
			if (index === -1)
				index = marker.handles.selectedHandleIndex;

			let i = index * 2;
			let newX;
			let newY;

			if (pointer)
			{
				let delta = this.getHandleDragDelta(marker, pointer);
				newX = delta.x;
				newY = delta.y;

				// Calculate how far the handle moved.
				deltaX = newX - coords[i];
				deltaY = newY - coords[i + 1];
			}
			else
			{
				newX = coords[i] + deltaX;
				newY = coords[i + 1] + deltaY;
			}

			// Update the coords with the handle location.
			console.log(`MapEditor::applyEditToHandleForPolygon ${deltaX},${deltaY} ${coords[i]},${coords[i + 1]}, ${newX},${newY}`)
			coords[i] = newX;
			coords[i + 1] = newY;
		}

		// Normalize the shape's coordinates if necessary to ensure that every coordinate's x and y value
		// will be relative to the upper left corner of the shape's bounding box after its dimensions
		// have changed as a result of editing the shape. For example, a coordinate becomes negative when
		// it has been dragged to the left or above the shape's original bounding rectangle which would
		// cause the rectangle to get larger. Normalization shifts all the coordinates to fit inside the
		// enlarged bounding rectangle. Later, the marker's center will get adjusted accordingly.
		let normalizedCoords = this.normalizeCoords(coords);
		let newSize = normalizedCoords.size;
		coords = normalizedCoords.coords;
		marker.shapeCoordsArray = coords;

		let oldSize = { w: marker.shapeW, h: marker.shapeH };

		// Update the polygon's size.
		marker.shapeW = newSize.w;
		marker.shapeH = newSize.h;

		this.applyEditToMarkerCenter(marker, index, oldSize, newSize, deltaX, deltaY);
	}

	applyEditToHandleForRectangle(marker, deltaX, deltaY, index, pointer)
	{
		// The four corners of a rectangle going clockwise from the upper left have indices: 0, 1, 2, 3.
		// How a rectangle is resized depends on which corner is being dragged. For example, dragging
		// corner 0 down and to the right makes the rectangle smaller, but dragging corner 2 down and
		// to the right makes the rectangle bigger. Depending on which corner was dragged and whether
		// it made the rectangle wider, narrower, taller, or shorter affects how the center point of the 
		// marker needs to be shifted. The calculations below take all this into account.

		if (pointer)
		{
			let delta = this.getHandleDragDelta(marker, pointer);
			deltaX = delta.x;
			deltaY = delta.y;

			switch (index)
			{
				case 1:
					deltaX -= marker.shapeW;
					break;

				case 2:
					deltaX -= marker.shapeW;
					deltaY -= marker.shapeH;
					break;

				case 3:
					deltaY -= marker.shapeH;
					break;
			}
		}

		// Reverse the delta direction as necessary to grow or shrink the rectangle as described above.
		if (index === 0 || index === 3)
			deltaX *= -1;
		if (index === 0 || index === 1)
			deltaY *= -1;

		let newSize = { w: marker.shapeW + deltaX, h: marker.shapeH + deltaY };

		// Prevent a rectangle from becoming too small.
		const minSide = 8;
		if (newSize.w < minSide)
			newSize.w = minSide;
		if (newSize.h < minSide)
			newSize.h = minSide;

		let oldSize = { w: marker.shapeW, h: marker.shapeH };

		// Update the rectangle's size and coords.
		marker.shapeW = newSize.w;
		marker.shapeH = newSize.h;
		marker.shapeCoords = `0,0,${marker.shapeW},${marker.shapeH}`;

		this.applyEditToMarkerCenter(marker, index, oldSize, newSize, deltaX, deltaY);
	}

	applyEditToMarkerCenter(marker, index, oldSize, newSize, deltaX, deltaY)
	{
		// This method deterimines precisley how the center of a polygon or rectangle should
		// be shifted left or right, and/or up or down, depending on which handle moved in
		// which direction. Be extremely careful and test thoroughly if changing this logic.
		let deltaCenterX = 0;
		let deltaCenterY = 0;

		// Determine how the bounding rectangle size changed.
		let deltaHalfW = (newSize.w - oldSize.w) / 2;
		let deltaHalfH = (newSize.h - oldSize.h) / 2;

		// When the bounding rectangle width got:
		// - Wider from the left or narrower from the right,  move the center point left.
		// - Wider from the right or narrower from the left, move the center point right.
		if (deltaHalfW > 0)
			deltaCenterX = deltaX < 0 ? -deltaHalfW : deltaHalfW;
		else if (deltaHalfW < 0)
			deltaCenterX = deltaX < 0 ? deltaHalfW : -deltaHalfW;

		// When the bounding rectangle height got:
		// - Taller from the top or shorter from the bottom,  move the center point up.
		// - Taller from the bottom or shorter from the top, move the center point down.
		if (deltaHalfH > 0)
			deltaCenterY = deltaY < 0 ? -deltaHalfH : deltaHalfH;
		else if (deltaHalfH < 0)
			deltaCenterY = deltaY < 0 ? deltaHalfH : -deltaHalfH;

		// Invert the delta as necessary for a rectangle depending on which handle
		// moved as explained in the comments for applyEditToHandleForRectangle.
		if (marker.shapeIsRectangle)
		{
			if (index === 0 || index === 3)
				deltaCenterX = -deltaCenterX;
			if (index === 0 || index === 1)
				deltaCenterY = -deltaCenterY;
		}

		// When the bounding rectangle's width or height is an odd number, the center x and/or y value will
		// contain a half pixel from having divided the odd dimension by 2. The half pixel either needs to be
		// discarded or it has to be rounded it up to a whole pixel depending. It's discarded when the dimension
		// is odd and rounded when the dimension is even. To see the effect of this, nudge a handle by 1 pixel
		// and notice how the nudged pixel moves 1 pixel each time, but the shape's center line only changes on
		// every other nudge. Without this logic, the center would shift on every nudge causing all the other
		// handles to wiggle side-to- side or up and down due to the fact that the shape cannot actually be
		// centered on a half pixel.
		if (marker.shapeW % 2 === 0)
			deltaCenterX = Math.floor(deltaCenterX);
		else
			deltaCenterX = Math.ceil(deltaCenterX);
		if (marker.shapeH % 2 === 0)
			deltaCenterY = Math.floor(deltaCenterY);
		else
			deltaCenterY = Math.ceil(deltaCenterY);

		// Convert the center deltas to map pixels if the marker does not zoom.
		if (!marker.markerZooms)
		{
			deltaCenterX = this.map.convertScreenToMap(deltaCenterX);
			deltaCenterY = this.map.convertScreenToMap(deltaCenterY);
		}

		// Shift the center of the shape.
		let x = marker.x + deltaCenterX;
		let y = marker.y + deltaCenterY;

		this.updateMarkerPosition(marker, x, y);
	}

	beginEditingMarker(marker)
	{
		//console.log(`MapEditor::beginEditingMarker ${marker.markerId}`);

		this.eraseEditLayer();
		this.eraseCrossHairRects();

		marker.isBeingEdited = true;
		this.markerBeingEdited = marker;
		this.addEditToHistory(new MarkerEdit(this.markerBeingEdited));
		this.markerBeingEdited.handles.initialize();

		// Recalculate the marker's bounds to not include extra for a thick line or a shadow/glow effects.
		// The size of those elements are excluded when a marker's isBeingEdited flag is set.
		// Also force the map to redraw so that if the marker has a shadow/glow effect it will get erased.
		this.markerBeingEdited.updateBounds();
		this.drawAndCacheUnselectedMarkers();

		this.map.drawMarkerOnly(this.markerBeingEdited, true);
		this.updateControls();
	}

	calculateFocusPoint()
	{
		// Get the coordinate of the focus point on the screen.
		let focusX_screen = this.map.convertMapToScreen(this.map.mapFocusX);
		let focusY_screen = this.map.convertMapToScreen(this.map.mapFocusY);
		let panX_screen = this.map.convertMapToScreen(this.map.panX_map);
		let panY_screen = this.map.convertMapToScreen(this.map.panY_map);
		let x = focusX_screen + panX_screen;
		let y = focusY_screen + panY_screen;
		return { x, y };
	}

	calculatePolygonBoundingRectangle(coords)
	{
		// Determine the width and height of the bounding rectangle that encloses all
		// of the polygon's coordinates. At the same time, determine whether any of the
		// points would be outside of the rectangle if positioned relative to the upper
		// left corner of the rectangle.
		let minX = Number.MAX_SAFE_INTEGER;
		let maxX = Number.MIN_SAFE_INTEGER;
		let minY = Number.MAX_SAFE_INTEGER;
		let maxY = Number.MIN_SAFE_INTEGER;

		for (let i = 0; i < coords.length; i += 2)
		{
			let x = coords[i];
			let y = coords[i + 1];

			minX = Math.min(x, minX);
			minY = Math.min(y, minY);
			maxX = Math.max(x, maxX);
			maxY = Math.max(y, maxY);
		}

		let w = maxX - minX + 1;
		let h = maxY - minY + 1;

		//console.log(`MapEditor::calculatePolygonBoundingRectangle ${w}x${h} ${minX},${minY} ${maxX},${maxY}`)

		// Return the size of the bounding rectangle along with the smallest x and y coordinates.
		// If neither one is zero, the coordinate values are NOT relative to the upper left corner
		// of the rectangle.
		return { size: { w, h }, minX, minY };
	}

	chooseStrokeColor(marker)
	{
		if (this.editingMarker)
			return '#cccccc';

		if (marker.isLocked)
			return 'black';

		if (this.hybridEditingEnabled)
			return "#cccccc";

		if (!marker.isAnchored)
			return 'green';

		return 'red';;
	}

	clearEditHistory()
	{
		this.editHistory = [];
		this.editHistoryIndex = -1;
	}

	convertBreakPointToHandle(marker, breakPoint)
	{
		// Get the coord for the breakpoint to use as the coord to insert into the polygon.
		let x = breakPoint.coord.x;
		let y = breakPoint.coord.y;

		// Determine whether the new coord should be inserted into the coords or appended to the end.
		let prevIndex = breakPoint.prev.index;
		let nextIndex = breakPoint.next.index;
		let insert = Math.abs(prevIndex - nextIndex) === 1;

		// Add the new coord.
		let coords = marker.shapeCoordsArray;
		if (insert)
		{
			let i = Math.max(prevIndex, nextIndex) * 2;
			coords.splice(i, 0, x, y);
		}
		else
		{
			coords.push(x);
			coords.push(y);
		}

		// Update and draw the polygon with the new coord.
		marker.shapeCoordsArray = coords;
		this.applyEditToHandle(0, 0);
		this.redrawSelectedMarkers();

		// Return the handle that was created for the new coords.
		let handle = marker.handles.getHandleForCoord(x, y);
		return handle;
	}

	createHtmlForMarkerNameField(duplicate)
	{
		// Create the first attempt at the new marker's name. When duplicating a marker, use the
		// name of the marker to be duplicated. Otherwise use the server-provided next marker suffix.
		// If the marker being duplicated is an exclusive marker, it's name will come back blank.
		let name = "";
		if (duplicate)
			name = this.getMarkerNameFromMarkerData(this.selectedMarkers.primaryMarker);
		if (name.length === 0)
			name = `Marker ${markerNameSuffix}`;

		let newName = name;

		// Determine if the name contains a suffix consisting of an integer inside parenthesis.
		// If so assume that it was previously added to make this marker's name unique. The
		// values \x28 and \x29 are the codes for left and right parens. If there is such a
		// suffix, extract the integer to use as the starting number to make this name unique.
		const re = /\x28[0-9]+\x29$/g;
		const found = name.match(re);
		let count = found ? parseInt(found[0].substring(1), 10) : 1;

		while (true)
		{
			// Exit when the new name is unique.
			if (!this.markerExists(newName))
				break;

			if (name.endsWith(`(${count})`))
			{
				// Replace the existing suffix with the integer bumped by 1.
				name = name.replace(`(${count})`, `(${count + 1})`)
				count += 1;
				newName = name;
			}
			else
			{
				// The name does not end with a number in parenthesis so give it one.
				count += 1;
				newName = `${name} (${count})`;
			}
		}

		let html = `
			<label class="map-editor-dialog-label" for="markerName">Marker Name:</label>
			<input class="map-editor-dialog-input" name="markerName" type="text" value="${newName}" required autocomplete="off" />`;
		
		return html;
	}

	createHtmlForHotspotFields(useExistingHostpot)
	{
		let suffix = 1;
		let id, title;

		while (true)
		{
			id = `H${suffix}`;
			title = `Hotspot ${suffix}`;
			if (!this.hotspotExists(id, title))
				break;
			suffix += 1;
		}

		let html = `
			<div id="hotspotFields" style="display:${useExistingHostpot ? "none" : "block"};">
			<label class="map-editor-dialog-label" for="hotspotTitle">Hotspot Title:</label>
			<input class="map-editor-dialog-input" name="hotspotTitle" type="text" value="${title}" required autocomplete="off" />
			<label class="map-editor-dialog-label" for="hotspotId">Hotspot Id:</label>
			<input class="map-editor-dialog-input" name="hotspotId" type="text" value="${id}" required autocomplete="off" />
			</div>`;

		return html;
	}

	createHtmlForHotspotRadioButtons(duplicate)
	{
		let lastChoice = maGetElementByPageId("LastHotspotChoice").value;

		const checked = " checked";
		let newChecked = lastChoice === "new" ? checked : "";
		let existingChecked = lastChoice === "existing" ? checked : "";;

		if (newChecked + existingChecked === "")
			newChecked = checked;

		let label = duplicate ? "Create a new hotspot that uses the duplicate marker" : "Create a new hotspot";

		let html = `
			<input${newChecked} type="radio" id="newHotspot" name="hotspotChoice" value="new" onchange="hotspotChoiceChanged(this)">
			<label class="map-editor-dialog-radio" for="newHotspot">${label}</label>`;

		if (hotspotData.length > 0)
		{
			label = duplicate ? "Assign the duplicate marker to an existing hotspot" : "Use an existing hotspot";
			html +=
				`<br><input${existingChecked} type="radio" id="existingHotspot" name="hotspotChoice" value="existing" onchange="hotspotChoiceChanged(this)">
				<label class="map-editor-dialog-radio" for="existingHotspot">${label}</label>`;
		}

		return html;
	}

	createHtmlForHotspotSelect(useExistingHotspot)
	{
		let html = `<div id="hotspotSelectList" style="display:${!useExistingHotspot ? "none" : "block"};">`;

		html += '<select class="map-editor-dialog-select" name="hotspotViewId" id="hotspotSelect"">';

		for (const hotspot of hotspotData)
		{
			let isSelected = hotspot.viewId === this.selectedMarkers.primaryViewId;
			let selected = isSelected ? "selected " : "";
			html += `<option ${selected}value=${hotspot.viewId}>${hotspot.title} (${hotspot.id})</option>`;
		}

		html += '</select></div>';
		return html;
	}

	createHtmlForMarkerSelectList()
	{
		let html = '<select class="map-editor-dialog-select" name="markerId" id="markerSelect" onchange="markerSelected()">';

		if (this.selectedMarkers.count > 0)
			defaultMarkerId = this.selectedMarkers.primaryMarker.markerId;

		let selectedMarkerData = null;
		for (const data of markerData)
		{
			let isSelected = data.id === defaultMarkerId || defaultMarkerId === 0;
			if (isSelected)
				selectedMarkerData = data;
			let selected = isSelected ? "selected " : "";
			html += `<option ${selected}value="${data.id}">${data.name}</option>`;
		}

		html += '</select >';

		// Handle the case where there was no match because the marker is exclusive
		// and could not be found because exclusive markers are not included in markerData.
		if (selectedMarkerData === null)
			selectedMarkerData = markerData[0];

		let imageId = selectedMarkerData.image;
		let src = imageId === "1" ? defaultMarkerImage : `${appRuntimeFolder}Markers/${imageId}.png`;
		html += `<div style="width:100%;background-color:#fff;padding:4px;"><img id="selectedMarkerImage" style="vertical-align:middle;" src="${src}"></div>`;

		return html;
	}

	createHtmlForMarkerStyleSelectList()
	{
		let defaultMarkerStyle;
		let html = '<select class="map-editor-dialog-select" name="markerStyleId" id="markerStyleSelect" onchange="markerStyleSelected()">';

		for (const data of markerStyleData)
		{
			let isDefault = data.id === defaultMarkerStyleId || defaultMarkerStyleId === 0;
			if (isDefault)
				defaultMarkerStyle = data;
			let selected = isDefault ? "selected " : "";
			html += `<option ${selected}value="${data.id}">${data.name}</option>`;
		}

		const src = `${appRuntimeFolder}MarkerStyles/${defaultMarkerStyle.image}.png`;
		html += '</select>';
		html += `<div style="width:100%;background-color:#fff;padding:4px;"><img id="selectedMarkerStyleImage" style="vertical-align:middle;" src="${src}"></div>`;

		return html;
	}

	createHtmlForShapeRadioButtons(lastShape)
	{
		const checked = " checked";
		let circleChecked = lastShape === "circle" ? checked  : "";
		let rectangleChecked = lastShape === "rectangle" ? checked : "";;
		let lineChecked = lastShape === "line" ? checked : "";;
		let polygonChecked = lastShape === "polygon" ? checked : "";

		if (circleChecked + rectangleChecked + lineChecked + polygonChecked === "")
			rectangleChecked = checked;

		let html = `
				<input${circleChecked} type="radio" id="circleShape" name="shape" value="circle"><label class="map-editor-dialog-radio" for="circleShape">Circle</label><br>
				<input${rectangleChecked} type="radio" id="rectangleShape" name="shape" value="rectangle"><label class="map-editor-dialog-radio" for="rectangleShape">Rectangle</label><br>
				<input${lineChecked} type="radio" id="lineShape" name="shape" value="line"><label class="map-editor-dialog-radio" for="lineShape">Line (choose a a marker style that has a visible line)</label><br>
				<input${polygonChecked} type="radio" id="polygonShape" name="shape" value="polygon"><label class="map-editor-dialog-radio" for="polygonShape">Polygon</label>`;

		return html;
	}

	createHtmlForStackRadioButtons()
	{
		let html = `
			<input checked type="radio" id="moveAbove" name="stackChoice" value="above"">
			<label class="map-editor-dialog-radio" for="moveAbove">Above all other markers</label><br>
			<input type="radio" id="moveBelow" name="stackChoice" value="below">
			<label class="map-editor-dialog-radio" for="moveBelow">Below all other markers</label>`;

		return html;
	}

	createMarkerPosition(marker, x, y)
	{
		return { "marker": marker, "x": x, "y": y };
	}

	get ctrlKeyIsDown()
	{
		return this.map.ctrlKey(window.event) || this.toggleSoftControlKeyEnabled;
	}

	async deleteHotspot()
	{
		let what = this.selectedMarkers.count === 1 ? "the selected hotspot" : `${this.selectedMarkers.count} hotspots`;
		let message = "Are you sure you want to delete " + what + "?</span>[*<span class='confirmSevereWarning'>WARNING:</span> Deleting hotspots cannot be undone.*]";
		let ok = await maAwaitConfirm(message, 'DELETE');
		if (ok)
			this.post("DeletedHotspots", this.selectedMarkers.viewIds.join(','));
	}

	deselectAllMarkers()
	{
		//console.log(`MapEditor::deselectAllMarkers`);

		this.selectedMarkers.removeAllMarkers();

		for (const marker of this.map.markers)
		{
			marker.appearsSelected = false;
			maShowMarkerThumbAsUnselected(marker.viewId);
		}

		this.map.flushMarkerLayerCache();
		this.map.drawMap();

		// Hide the crosshairs and the outlines of any markers that were selected.
		this.eraseEditLayer();
		this.viewIdForMarkerWithCrosshairs = 0;

		maShowMarkerInHotspotList(0);
		this.updateControls();
	}

	deselectMarker(viewId)
	{
		let marker = this.map.getMarker(viewId);
		marker.appearsSelected = false;
		this.map.drawMarkerAndNeighbors(marker, false);
		maShowMarkerThumbAsUnselected(viewId);
	}

	detectPanTarget(pointer)
	{
		this.draggingCrosshairs = false;
		this.draggingMarker = false;
		this.draggingHandle = false;

		let handleBeingDragged = this.getHandleUnderPointer(pointer);
		if (handleBeingDragged)
		{
			//console.log(`MapEditor::detectPanTarget START DRAGGING HANDLE`);

			if (handleBeingDragged.isBreakPoint)
			{
				let marker = this.markerBeingEdited;
				let newHandle = this.convertBreakPointToHandle(marker, handleBeingDragged);
				marker.handles.handleUnderPointer = newHandle;
			}

			this.draggingHandle = true;
			this.dragHandleDistanceX = 0;
			this.dragHandleDistanceY = 0;
			let handles = this.selectedMarkers.primaryMarker.handles;
			handles.handleBeingDragged = handles.handleUnderPointer;
			handles.setOnlySelectedHandle(handles.handleBeingDragged);
			return;
		}

		let markerBeingDragged = this.map.getMarkerUnderPointer(pointer);
		this.viewIdForMarkerBeingDragged = markerBeingDragged === null ? 0 : markerBeingDragged.viewId;

		if (this.crosshairRectUnderPointer !== null && this.viewIdForMarkerBeingDragged !== this.viewIdForMarkerWithCrosshairs)
		{
			// The user clicked on crosshairs and the pointer is not over the marker that belongs to the crosshairs.
			// Initiate dragging of the crosshairs. When the pointer is over the crosshairs marker, but not any other
			// marker, the code in the next else gets executed so the user can drag the marker in any direction
			// instead of only being able to drag the crosshairs left/right or up/down.
			this.draggingCrosshairs = true;
		}
		else if (this.viewIdForMarkerBeingDragged !== 0)
		{
			// The user clicked on a marker. Initiate marker dragging unless the marker is locked or being
			// edited. Though it's okay to drag a marker being edited, this prevents you from unintentionally
			// draggin the marker when attempting to drag a handle. Ctrl-drag overrides this restriction.
			let marker = this.map.getMarker(this.viewIdForMarkerBeingDragged);
			this.draggingMarker = !marker.isLocked && (!this.editingMarker || this.ctrlKeyIsDown);

			// Select the marker which will cause it's outline and crosshairs to be drawn.
			this.setMarkerSelected(this.viewIdForMarkerBeingDragged, pointer);

			if (this.isGalleryEditor)
			{
				// Highlight the clicked marker in the list of markers.
				maGalleryMarkerSelected(this.viewIdForMarkerBeingDragged);
			}
		}
	}

	drawAndCacheUnselectedMarkers()
	{
		// Erase all the markers and flush the cache.
		this.map.eraseMarkerLayer();
		this.map.flushMarkerLayerCache();

		// Draw the unselected markers.
		for (const marker of this.map.markers)
		{
			if (this.selectedMarkers.contains(marker.viewId))
				continue;
			this.map.drawMarkerOnly(marker, false);
		}

		// Save the cache for the unselected markers. Now, when the user drags or nudges selected markers,
		// the new positions can be drawn on top of the cached appearance of the unselected markers which
		// is much faster than if the unselected markers had to be redrawn after each movement.
		this.map.createMarkerLayerCache();
	}

	drawCrosshairs(ctx, x, y, strokeColor, lineDash = [])
	{
		// Draw crosshairs through the x,y.

		// Set the location and size of the rectangle for the horizontal crosshair./ This and the
		// vertical crosshair rectangle are used to do hit testing when mousing over the crosshairs.
		this.crosshairRectH.x = 0;
		this.crosshairRectH.y = y - 1;
		this.crosshairRectH.w = this.map.editLayer.width;
		this.crosshairRectH.h = 3;

		// Set the location and size of the rectangle for the vertical crosshair.
		this.crosshairRectV.x = x - 1;
		this.crosshairRectV.y = 0;
		this.crosshairRectV.w = 3;
		this.crosshairRectV.h = this.map.editLayer.height;

		// Draw the crosshairs first as a transparent white rectangle.
		ctx.fillStyle = "white";
		ctx.globalAlpha = 0.25;
		ctx.fillRect(this.crosshairRectH.x, this.crosshairRectH.y, this.crosshairRectH.w, this.crosshairRectH.h);
		ctx.fillRect(this.crosshairRectV.x, this.crosshairRectV.y, this.crosshairRectV.w, this.crosshairRectV.h);

		// Draw a solid red line through each crosshair. Offset it by a half pixel to get a crisp line.
		x += 0.5;
		y += 0.5;
		ctx.beginPath();
		ctx.setLineDash(lineDash);
		ctx.strokeStyle = strokeColor;
		ctx.lineWidth = 1;
		ctx.globalAlpha = 1.0;

		// Draw the red line for the horizontal crosshair.
		ctx.moveTo(0, y);
		ctx.lineTo(this.map.editLayer.width, y);
		ctx.stroke();

		// Draw the red line for the vertical crosshair.
		ctx.moveTo(x, 0);
		ctx.lineTo(x, this.map.editLayer.height);
		ctx.stroke();
	}

	drawOutlineForHybrid(marker)
	{
		if (!this.isMapEditor)
			return;

		// Set the drawing properties. The stroke is a solid red (no opacity), 1 pixel, line unless the
		// marker is locked, in which case, draw a dashed line. Draw non-anchored marker's outline in green.
		let ctx = this.map.editLayerContext;
		ctx.lineWidth = 2;
		ctx.globalAlpha = 1.0;
		ctx.strokeStyle = "cyan";
		ctx.setLineDash([10, 10]);

		const pad = 8;
		let outline = marker.getBounds();
		const x = outline.cornerX_screen - pad;
		const y = outline.cornerY_screen - pad;
		ctx.strokeRect(x, y, outline.w_screen + pad * 2, outline.h_screen + pad * 2);
		//console.log(`MapEditor::drawOutlineForHybrid ${x},${y}`);
	}

	drawOutlineForMarker(marker)
	{
		//console.log(`MapEditor::drawOutlineForMarker ${marker.markerId}`);

		if (this.hybridEditingEnabled)
		{
			this.drawOutlineForHybrid(this.hybridMarkerBeingEdited);

			// When the marker is the hybrid marker, not a pseudo marker, there's nothing else to be drawn.
			if (marker === this.hybridMarkerBeingEdited)
				return;
		}

		// Don't show the outline and crosshairs when edting a marker because they are distracting
		// and really not needed since you are not likely to drag them while editing handles.
		if (this.editingMarker && !this.map.tour.flagShowCrosshairsForMarkerBeingEdited)
			return;

		let ctx = this.map.editLayerContext;

		// Set the drawing properties. The stroke is a solid red (no opacity), 1 pixel, line unless the
		// marker is locked, in which case, draw a dashed line. Draw non-anchored marker's outline in green.
		ctx.lineWidth = 1;
		ctx.globalAlpha = 1.0;

		let strokeColor = this.chooseStrokeColor(marker);
		ctx.strokeStyle = strokeColor;

		// Draw a dash line for a locked marker, otherwise, draw a solid line.
		let lineDash = marker.isLocked ? [3, 3] : [];
		ctx.setLineDash(lineDash);

		// Get a copy of the marker bounds to use for the outline.
		let outline = marker.getBounds();

		// Rotate the canvas if the marker is rotated.
		let rotate = marker.rotationRadians !== 0;
		if (rotate)
		{
			ctx.save();

			// Translate the context origin to be the center of the marker and rotate the canvas around that point.
			let centerX = outline.anchorX_screen;
			let centerY = outline.anchorY_screen;
			ctx.translate(centerX, centerY);
			ctx.rotate(marker.rotationRadians);

			// Now that the rotation has been done properly, translate the context back to its original origin.
			// When the outline gets drawn on the rotated canvas, it will match the perimeter of the the rotated marker.
			ctx.translate(-centerX, -centerY);
		}

		// Draw the outline. Offset it by a half pixel to get a crisp line.
		if (this.isMapEditor)
		{
			if (marker.shapeIsHybrid)
				ctx.setLineDash([10, 10]);
			const x = outline.cornerX_screen + 0.5;
			const y = outline.cornerY_screen + 0.5;
			ctx.strokeRect(x, y, outline.w_screen, outline.h_screen);
		}

		// Restore the canvas to its unrotated state.
		if (rotate)
			ctx.restore();

		// Draw crosshairs through the primary selected marker.
		if (marker.viewId === this.selectedMarkers.primaryViewId)
		{
			this.viewIdForMarkerWithCrosshairs = marker.viewId;
			this.drawCrosshairs(ctx, outline.anchorX_screen, outline.anchorY_screen, strokeColor, lineDash);
		}
	}

	dynamicToolMessage(actionId, message)
	{
		let primaryMarker = this.selectedMarkers.count === 1 ? this.selectedMarkers.primaryMarker : null;

		switch (actionId)
		{
			case this.ACTION_REMOVE:
				if (this.editingMarker)
					return "Remove the selected handle"
				if (this.hybridEditingEnabled)
					return "Remove the selected shape(s) from the hybrid marker";
				return message;
				break;

			case this.ACTION_CONVERT_HYBRID:
				if (this.hybridEditingEnabled)
					return "Exit hybrid marker editing mode";
				if (!this.oneShapeMarkerSelected)
					return "";
				if (primaryMarker.shapeIsHybrid)
					return "Convert the hybrid marker to a shape marker";
				if (this.oneShapeMarkerSelected)
					return "Convert the shape marker to a hybrid marker";
				break;

			default:
				return message;
		}
	}

	get editingMarker()
	{
		return this.markerBeingEdited_ !== null;
	}

	set editingMarker(editing)
	{
		if (!editing)
			this.markerBeingEdited_ = null;
	}

	enableHybridEditing(enable)
	{
		//console.log(`MapEditor::enableHybridEditing ${enable}`);

		if (enable)
		{
			this.hybridMarkerBeingEdited = this.selectedMarkers.primaryMarker;
			this.deselectAllMarkers();
			this.map.enterHybridEditingMode(true, this.hybridMarkerBeingEdited);
			this.hybridEditingEnabled = true;
			this.drawOutlineForMarker(this.hybridMarkerBeingEdited);
		}
		else
		{
			this.finishEditingMarker();
			this.deselectAllMarkers();
			this.map.enterHybridEditingMode(false, this.hybridMarkerBeingEdited);
			this.updateMarkerAndMapAfterEdit(this.hybridMarkerBeingEdited);
			let hybridMarkerViewId = this.hybridMarkerBeingEdited.viewId;
			this.hybridMarkerBeingEdited = null;
			this.hybridEditingEnabled = false;
			this.setMarkerSelected(hybridMarkerViewId);
		}

		this.updateControls();
	}

	eraseCrossHairRects()
	{
		this.crosshairRectH = { x: 0, y: 0, w: 0, h: 0 };
		this.crosshairRectV = { x: 0, y: 0, w: 0, h: 0 };
	}

	eraseEditLayer()
	{
		//console.log(`MapEditor::eraseEditLayer`);
		this.map.editLayerContext.clearRect(0, 0, this.map.editLayer.width, this.map.editLayer.height);
	}

	eraseHandlesLayer()
	{
		//console.log(`MapEditor::eraseHandlesLayer`);
		this.map.handlesLayerContext.clearRect(0, 0, this.map.editLayer.width, this.map.editLayer.height);
	}

	featureDisabled(feature)
	{
		if (isPlusOrProPlan && this.map.tour.v4)
			return false;

		feature = `<b>${feature}</b>`;

		if (this.map.tour.enableV3Compatibility)
		{
			let message = `[The ${feature} feature is not enabled for V3 tours.][<a target="_blank" `;
			message += `href="https://mapsalive.com/docs/about-v3/#converting-from-v3-to-v4">Learn about converting a V3 tour to V4.</a>]`;
			maAlert(message);
			return true;
		}

		maAlert(`]The ${feature} feature requires a Plus or Pro plan.[To upgrade, choose [@Account > Updgrade@] from the menu.]`)
		return true
	}

	finishEditingMarker()
	{
		if (!this.editingMarker)
			return;

		console.log(`MapEditor::finishEditingMarker ${this.markerBeingEdited.markerId}`);

		this.eraseHandlesLayer();

		this.markerBeingEdited.isBeingEdited = false;

		// Restore the marker's original bounds in case they had are extended for a thick line or a shadow/glow.
		this.markerBeingEdited.updateBounds();

		this.editingMarker = false;

		this.editHistory = [];
		this.editHistoryIndex = -1;
	}

	getControl(actionId)
	{
		let controls = this.controls.filter(obj => { return obj.actionId === actionId });
		return controls[0];
	}

	getCrosshairUnderPointer(pointer)
	{
		// Pad the crosshairs hit area to make them easier to hit with the mouse or a finger.
		let padding = this.map.tour.isTouchDevice ? 16 : 8;

		if (pointer.y >= this.crosshairRectH.y - padding && pointer.y < this.crosshairRectH.y + this.crosshairRectH.h + padding)
			this.crosshairRectUnderPointer = this.crosshairRectH;
		else if (pointer.x >= this.crosshairRectV.x - padding && pointer.x < this.crosshairRectV.x + this.crosshairRectV.w + padding)
			this.crosshairRectUnderPointer = this.crosshairRectV;
		else
			this.crosshairRectUnderPointer = null;
	}

	getHandleDragDelta(marker, pointer)
	{
		// Get the distance from a shape's upper left corner to the handle being dragged.
		// The pointer is the mouse location in screen pixels. Convert it to a map location.
		let b = marker.getBounds();
		let deltaX = pointer.x - b.cornerX_screen;
		let deltaY = pointer.y - b.cornerY_screen;
		if (marker.markerZooms)
		{
			deltaX = this.map.convertScreenToMap(deltaX);
			deltaY = this.map.convertScreenToMap(deltaY);
		}
		return { x: deltaX, y: deltaY };
	}

	getHandleUnderPointer(pointer)
	{
		if (!this.editingMarker)
			return false;

		if (this.selectedMarkers.count === 0)
			return false;

		let marker = this.selectedMarkers.primaryMarker
		marker.handles.handleUnderPointer = null;

		let handleList = marker.handles.getHandleList();
		const side = marker.handles.HANDLE_SIDE;

		// Enlarge the hit area to make the handles easier to select.
		let extra = 4;

		for (const handle of handleList)
		{
			if (pointer.x < handle.xh - extra || pointer.x > handle.xh + side + extra)
				continue;
			if (pointer.y < handle.yh - extra || pointer.y > handle.yh + side + extra)
				continue;
			marker.handles.handleUnderPointer = handle;
			break;
		}

		extra = 6 + side;
		for (const breakPoint of marker.handles.breakPoints)
		{
			if (pointer.x < breakPoint.x - extra || pointer.x > breakPoint.x + extra)
				continue;
			if (pointer.y < breakPoint.y - extra || pointer.y > breakPoint.y + extra)
				continue;

			marker.handles.handleUnderPointer = breakPoint;
			break;
		}

		//console.log(`MapEditor::getHandleUnderPointer ${marker.handles.handleUnderPointer ? marker.handles.handleUnderPointer.index : "none"}`);

		return marker.handles.handleUnderPointer;
	}

	getMarkerNameFromMarkerData(marker)
	{
		let data = markerData.filter(obj => { return obj.id === marker.markerId });
		return data.length >= 1 ? data[0].name : "";
	}

	handleEventClick(event)
	{
		if (!this.editingMarker)
			return;

		if (this.ignoreClick)
		{
			// Set the comment where this flag is set in stopDragging().
			this.ignoreClick = false;
			return;
		}

		let marker = this.selectedMarkers.primaryMarker;

		if (marker.handles.handleUnderPointer === null)
		{
			if (this.ctrlKeyIsDown && this.viewIdForMarkerUnderPointer === marker.viewId)
			{
				// The user ctrl clicked on the marker. Deselect all the handles.
				marker.handles.setSelectedHandle(null);
				this.redrawSelectedMarkers();
				this.updateControls();
			}
			return;
		}

		// Ignore a click on a breakpoint handle. Breakpoints can only be dragged.
		if (marker.handles.handleUnderPointer.isBreakPoint)
			return;

		if (marker.handles.selectedHandleCount === 0)
		{
			// When no handles are selected, a click or ctrl-click selects the clicked handle.
			marker.handles.setSelectedHandle(marker.handles.handleUnderPointer);
		}
		else
		{
			// When ctrl-clicking a handle, if it's selected, deselect it, otherwise, deselect it.
			// When there's no ctrl key, deselect all other handles and select the clicked handle.
			if (this.ctrlKeyIsDown)
				marker.handles.setSelectedHandle(marker.handles.handleUnderPointer, true);
			else
				marker.handles.setOnlySelectedHandle(marker.handles.handleUnderPointer);
		}

		this.redrawSelectedMarkers();
		this.updateControls();
	}

	handleEventDoubleClick(event)
	{
		//console.log(`MapEditor::handleEventDoubleClick`);
		let marker = this.selectedMarkers.primaryMarker;
		if (marker === null)
			return;

		if (this.editingMarker)
		{
			if (this.viewIdForMarkerUnderPointer === this.markerBeingEdited.viewId)
				this.performAction(this.ACTION_FINISH_EDITING);
		}
		else
		{
			if (this.oneShapeMarkerSelected)
				this.performAction(this.ACTION_EDIT_MARKER);
		}
	}

	handleEventKeyDown(event)
	{
		//console.log(`handleEventKeyDown ${event.key} ${event.keyCode}`);

		if (this.map.tour.disableKeyboardShortcuts)
			return;

		if (showingDialog)
			return;

		// No keystorkes are supported for the gallery editor.
		if (this.isGalleryEditor)
			return;

		let nudgeAction = 0;
		let consumeKey = true;

		switch (event.keyCode)
		{
			case 9: // Tab key
				this.selectAdjacentHandle(event.shiftKey);
				break;
			
			case 13: // Enter key
				if (this.editingMarker)
					this.performAction(this.ACTION_FINISH_EDITING);
				else if (this.hybridEditingEnabled)
					this.performAction(this.ACTION_CONVERT_HYBRID);
				else
					consumeKey = false;
				break;

			case 27: // Escape key
				if (this.inMapFocusingState)
					this.setMapFocusingState(false);
				else if (this.editingMarker)
					this.performAction(this.ACTION_CANCEL_EDITING);
				else
					consumeKey = false;
				break;

			case 37:
				nudgeAction = this.ACTION_NUDGE_LEFT;
				break;

			case 38:
				nudgeAction = this.ACTION_NUDGE_UP;
				break;

			case 39:
				nudgeAction = this.ACTION_NUDGE_DOWN;
				break;

			case 40:
				nudgeAction = this.ACTION_NUDGE_RIGHT;
				break;

			case 46: // Delete key
				if (this.editingMarker && !this.okToRemoveHandle(this.markerBeingEdited))
					consumeKey = false;
				else
					this.performAction(this.ACTION_REMOVE);
				break;

			case 67: // 'c' key
			case 82: // 'r' key
				if (this.selectedMarkers.count >= 2 || this.editingMarker && this.markerBeingEdited.handles.selectedHandleCount >= 2)
				{
					const action = event.keyCode === 67 ? this.ACTION_ALIGN_COLUMN : this.ACTION_ALIGN_ROW;
					this.performAction(action);
				}
				break;

			case 89: // 'y' key
				
				if (this.ctrlKeyIsDown)
					this.performAction(this.ACTION_REDO_EDIT);
				else
					consumeKey = false;
				break;

			case 90: // 'z' key
				if (this.ctrlKeyIsDown)
					this.performAction(this.ACTION_UNDO_EDIT);
				else
					consumeKey = false;
				break;

			default:
				consumeKey = false;
				break;
		}

		// Keep the keystroke from getting to the browser.
		if (consumeKey)
			event.preventDefault();

		if (nudgeAction)
			this.performAction(nudgeAction);
	}

	handleEventMapEditorPan(panEventType, pointer)
	{
		//console.log(`MapEditor::handleEventMapEditorPan ${panEventType} ${pointer.x},${pointer.y}`);

		// Ignore pan while a marker is being dropped onto the map.
		if (this.animatingMarker)
			return true;

		switch (panEventType)
		{
			case 'panstart':
				this.detectPanTarget(pointer);
				if (!this.draggingCrosshairs && !this.draggingMarker && !this.draggingHandle)
					return false;

				// Record the pointer location so we'll know how far to drag on subsequent 'pan' events.
				this.currentPointerLocation = pointer;
				this.lastDragLocation = pointer;
				this.dragDeltaX = 0;
				this.dragDeltaY = 0;

				// Flush the marker layer cache so that subsquent optimization logic will know
				// if and when it should cache and resuse the marker state during dragging.
				this.map.flushMarkerLayerCache();

				// Initiate dragging.
				this.performDragOnEachAnimationFrame();
				break;

			case 'pan':
				// Ignore the pan event if the panstart event occurred while a marker was being
				// positioned onto the map which would have prevented lastDragLocation from being set.
				if (this.lastDragLocation === null)
					return true;

				// Determine how far the pointer has moved since the last drag was performed. The next
				// drag will occur on the next call to performDragOnEachAnimationFrame. If this pan
				// method is called in between animation frames because the pointer is moving quickly,
				// intermediate deltas will be ignored, but they'll keep growing so that when
				// performDragOnEachAnimationFrame is called, the most recent deltas will be used.
				this.currentPointerLocation = pointer;
				this.dragDeltaX = this.currentPointerLocation.x - this.lastDragLocation.x;
				this.dragDeltaY = this.currentPointerLocation.y - this.lastDragLocation.y;

				// Force the recalculation and drawing of the edit handles at the new pan location.
				if (this.editingMarker)
					this.markerBeingEdited.handles.invalidateHandles();

				break;

			case 'panend':
				this.map.markerLayer.style.cursor = this.map.pointerIsOverMarker ? "pointer" : "auto";
				this.stopDragging();
				break;

			default:
				console.log(`MapEditor::handleEventMapEditorPan UNEXPECTED PAN TYPE ${panEventType}`);
				this.stopDragging();
		}

		return true;
	}

	handleEventMapEditorTap(tappedMarkerViewId, pointer)
	{
		// Ignore tap while a marker is being dropped onto the map.
		if (this.animatingMarker)
			return false;

		if (this.inMapFocusingState)
		{
			this.recordMapState({ applyFocus: true, pointer: pointer });
			return;
		}

		//console.log(`MapEditor::handleEventMapEditorTap ${tappedMarkerViewId}`);

		if (tappedMarkerViewId !== 0)
		{
			// The user tapped on a marker.

			let marker = this.map.getMarker(tappedMarkerViewId);
			if (this.editingMarker)
			{
				if (marker.markerId === this.markerBeingEdited || marker.typeIsSymbol || this.ctrlKeyIsDown)
					return;
			}

			// When the user taps on a selected marker, do nothing unless the ctrl key is down,
			// in which case, deselect that marker.
			if (this.selectedMarkers.contains(marker.viewId))
			{
				if (this.ctrlKeyIsDown)
				{
					this.selectedMarkers.removeMarker(marker.viewId);
					this.deselectMarker(marker.viewId);
					this.redrawSelectedMarkers();
					this.updateControls();
				}
				return;
			}

			// Select the marker which will cause its outline and crosshairs to be drawn.
			this.setMarkerSelected(tappedMarkerViewId, pointer);

			if (this.isGalleryEditor)
			{
				// Highlight the clicked marker in the list of markers.
				maGalleryMarkerSelected(tappedMarkerViewId);
			}

			this.turnOffSoftControlKey();
			this.updateControls();
		}
		else if (this.ctrlKeyIsDown)
		{
			// The user control-clicked on the map, not on a marker.
			if (this.editingMarker)
				return;

			this.turnOffSoftControlKey();
			this.finishEditingMarker();
			this.deselectAllMarkers();

			if (this.hybridEditingEnabled)
				this.redrawHybrid();
		}
		else
		{
			// The user clicked on the map.
			return;
		}

		return;
	}

	handleEventMouseUp()
	{
		console.log('MapEditor::handleEventMouseUp');

		// A mouseup even terminates dragging.
		this.stopDragging();
	}

	handleEventPointerOverMapEditor(markerUnderPointerViewId, pointer)
	{
		if (this.draggingCrosshairs)
			return;

		if (this.isGalleryEditor)
		{
			// The Gallery Editor does nothing special with mouseover events.
			this.showMarkerInformation(markerUnderPointerViewId);
			return;
		}

		if (this.editingMarker)
		{
			// Draw each of the marker's handles which change appearance depending on whether under the pointer.
			this.getHandleUnderPointer(pointer);
			this.map.graphics.drawHandles(this.markerBeingEdited);
		}

		this.getCrosshairUnderPointer(pointer);
		this.viewIdForMarkerUnderPointer = markerUnderPointerViewId;

		this.setCursor();

		// Display information about the marker in the control panel.
		this.showMarkerInformation(this.viewIdForMarkerUnderPointer);
	}

	hotspotExists(id, title)
	{
		for (const hotspot of hotspotData)
		{
			if (hotspot.id.toUpperCase() === id.toUpperCase() || hotspot.title.toUpperCase() === title.toUpperCase())
				return true;
		}
		return false;
	}

	hotspotSelectedWhileEditing()
	{
		maAlert("You cannot select a hotspot while editing a marker.");
	}

	get ignoreMessage()
	{
		return this.inMapFocusingState;
	}

	initializeActions()
	{
		this.currentActionId = 0;
		this.previousActionId = 0;

		this.ACTION_TOGGLE_SOFT_CONTROL_KEY = 1;
		this.ACTION_SELECT_ALL_MARKERS = 2;
		this.ACTION_CHANGE_STACK_ORDER = 3;
		this.ACTION_ALLOW_OVER_ZOOM = 4;

		this.ACTION_ROTATE_LEFT = 10;
		this.ACTION_ROTATE_RIGHT = 11;
		this.ACTION_NUDGE_LEFT = 12;
		this.ACTION_NUDGE_UP = 13;
		this.ACTION_NUDGE_DOWN = 14;
		this.ACTION_NUDGE_RIGHT = 15;

		this.ACTION_ALIGN_COLUMN = 20;
		this.ACTION_ALIGN_ROW = 21;

		this.ACTION_TOGGLE_APPEARANCE = 30;
		this.ACTION_TOGGLE_TRANSPARENCY = 31;
		this.ACTION_TOGGLE_LOCK_MARKER = 32;
		this.ACTION_GO_TO_EDIT_SCREEN = 33;
		this.ACTION_REMOVE = 34;
		this.ACTION_SET_MAP_FOCUS = 35;
		this.ACTION_SHOW_MAP_FOCUS = 36;
		this.ACTION_REPLACE_MARKER = 37;
		this.ACTION_REPLACE_MARKER_STYLE = 38;

		this.ACTION_ADD_NEW_HOTSPOT = 40;
		this.ACTION_CREATE_NEW_MARKER = 41;
		this.ACTION_DUPLICATE_MARKER = 42;

		this.ACTION_EDIT_MARKER = 50;
		this.ACTION_CONVERT_HYBRID = 51;
		this.ACTION_FINISH_EDITING = 52;
		this.ACTION_UNDO_EDIT = 53;
		this.ACTION_REDO_EDIT = 54;
		this.ACTION_CANCEL_EDITING = 55;
	}

	initializeControls()
	{
		this.lastMessage = '';
		this.mapControlsMessageArea = document.getElementById('mapControlsMessageArea');
		this.mapControlsMessageArea.innerHTML = this.lastMessage

		this.controls = [];

		this.controls.push(new MapControl("RotateLeft", this.ACTION_ROTATE_LEFT));
		this.controls.push(new MapControl("RotateRight", this.ACTION_ROTATE_RIGHT));

		this.controls.push(new MapControl("ToggleAppearance", this.ACTION_TOGGLE_APPEARANCE));
		this.toggleAppearanceEnabled = false;

		this.controls.push(new MapControl("GoToEditScreen", this.ACTION_GO_TO_EDIT_SCREEN));
		if (this.isGalleryEditor)
		{
			this.updateControls();
			return;
		}

		this.controls.push(new MapControl("ToggleSoftControlKey", this.ACTION_TOGGLE_SOFT_CONTROL_KEY));
		this.toggleSoftControlKeyEnabled = false;

		this.controls.push(new MapControl("AllowOverZoom", this.ACTION_ALLOW_OVER_ZOOM));
		this.allowOverZoomEnabled = this.map.currentMapScale > 1.0;
		if (this.allowOverZoomEnabled)
		{
			this.activateControl(this.ACTION_ALLOW_OVER_ZOOM, true);
			this.map.allowOverZoom = true;
		}

		this.controls.push(new MapControl("SelectAllMarkers", this.ACTION_SELECT_ALL_MARKERS));
		this.controls.push(new MapControl("ChangeStackOrder", this.ACTION_CHANGE_STACK_ORDER));

		this.controls.push(new MapControl("NudgeLeft", this.ACTION_NUDGE_LEFT));
		this.controls.push(new MapControl("NudgeUp", this.ACTION_NUDGE_UP));
		this.controls.push(new MapControl("NudgeDown", this.ACTION_NUDGE_RIGHT));
		this.controls.push(new MapControl("NudgeRight", this.ACTION_NUDGE_DOWN));

		this.controls.push(new MapControl("AlignColumn", this.ACTION_ALIGN_COLUMN));
		this.controls.push(new MapControl("AlignRow", this.ACTION_ALIGN_ROW));

		this.controls.push(new MapControl("ToggleTransparency", this.ACTION_TOGGLE_TRANSPARENCY));
		this.toggleTransparencyEnabled = false;

		this.controls.push(new MapControl("ToggleLockMarker", this.ACTION_TOGGLE_LOCK_MARKER));
		this.controls.push(new MapControl("Remove", this.ACTION_REMOVE));

		if (!this.map.inMapEditorWithNonZoomableV3Map)
		{
			this.controls.push(new MapControl("SetMapFocus", this.ACTION_SET_MAP_FOCUS));
			if (this.map.tour.v4)
				this.controls.push(new MapControl("ShowMapFocus", this.ACTION_SHOW_MAP_FOCUS));
		}

		this.controls.push(new MapControl("ReplaceMarker", this.ACTION_REPLACE_MARKER));
		this.controls.push(new MapControl("ReplaceMarkerStyle", this.ACTION_REPLACE_MARKER_STYLE));
		this.controls.push(new MapControl("AddNewHotspot", this.ACTION_ADD_NEW_HOTSPOT));
		this.controls.push(new MapControl("CreateNewMarker", this.ACTION_CREATE_NEW_MARKER));
		this.controls.push(new MapControl("DuplicateMarker", this.ACTION_DUPLICATE_MARKER));
		this.controls.push(new MapControl("EditMarker", this.ACTION_EDIT_MARKER));
		this.controls.push(new MapControl("ConvertHybrid", this.ACTION_CONVERT_HYBRID));
		this.controls.push(new MapControl("FinishEditing", this.ACTION_FINISH_EDITING));
		this.controls.push(new MapControl("UndoEdit", this.ACTION_UNDO_EDIT));
		this.controls.push(new MapControl("RedoEdit", this.ACTION_REDO_EDIT));
		this.controls.push(new MapControl("CancelEditing", this.ACTION_CANCEL_EDITING));

		this.updateControls();
	}

	mapWasPanned()
	{
		if (this.hybridEditingEnabled)
			this.redrawHybrid()
		else
			this.redrawSelectedMarkers();
	}

	mapZoomStateChanged()
	{
		this.updateControls();

		if (this.editingMarker)
			this.markerBeingEdited.handles.invalidateHandles();

		if (this.map.tour.v4)
			this.recordMapState({ updateAfterPanZoom: true });
	}

	get markerBeingEdited()
	{
		return this.markerBeingEdited_;
	}

	set markerBeingEdited(marker)
	{
		//console.log(`MapEditor::markerBeingEdited ${marker.markerId}`);
		this.markerBeingEdited_ = marker;
	}

	markerExists(name)
	{
		for (const data of markerData)
		{
			if (data.name.toUpperCase() === name.toUpperCase())
				return true;
		}
		return false;
	}

	moveMarkers(markerPositions)
	{
		//console.log(`MapEditor::moveMarkers ${markerPositions.length} markers`);

		// Don't allow gallery markers to be moved.
		if (this.isGalleryEditor)
			return;

		for (const markerPosition of markerPositions) 
		{
			let marker = markerPosition.marker;
			//console.log(`Move ${marker.viewId} from ${marker.x},${marker.y} to ${markerPosition.x},${markerPosition.y}`);

			// Change the marker's position.
			this.updateMarkerPosition(marker, markerPosition.x, markerPosition.y);
		}

		if (markerPositions.length === 1)
		{
			let marker = markerPositions[0].marker;
			this.showMarkerCoordsMessage(marker);
			this.updateControls();
		}
		else
		{
			this.showMarkerCoordsMessage(null);
			this.updateControls();
		}

		if (this.hybridEditingEnabled)
		{
			// Update the hybrid marker to reflect the new positions of the pseudo markers.
			this.map.createHybridMarkerFromPseudoMarkers(this.hybridMarkerBeingEdited);
			this.reportMarkerEdit(this.hybridMarkerBeingEdited);
		}
		else
		{
			// Report the new marker positions, but not when editing a hybrid marker.
			this.map.reportMarkerCoords();
		}

		// Draw the unselected markers.
		if (this.map.markerLayerCacheIsStale)
			this.drawAndCacheUnselectedMarkers();
		else
			this.map.drawCachedMarkerLayer();

		// Erase the crosshairs from the previous move.
		this.eraseEditLayer();

		// Draw the selected markers.
		for (const viewId of this.selectedMarkers.viewIds)
		{
			let marker = this.map.getMarker(viewId);
			marker.appearsSelected = true;
			this.map.drawMarkerOnly(marker, true);
			this.drawOutlineForMarker(marker);
		}

		if (this.hybridEditingEnabled)
			this.drawOutlineForMarker(this.hybridMarkerBeingEdited);

		maChangeDetected();
	}

	moveSelectedMarkers(deltaX, deltaY, pointer)
	{
		//console.log(`MapEditor::moveSelectedMarkers ${deltaX},${deltaY}`);

		// Verify that all selected markers can be moved. If the center of any one of them
		// would move off of the canvas, don't ok the movement of any of them.
		let markerPositions = [];
		for (const viewId of this.selectedMarkers.viewIds) 
		{
			let marker = this.map.getMarker(viewId);

			if (marker.isLocked)
				continue;

			let x = marker.x + deltaX;
			let y = marker.y + deltaY;

			// Determine if the new position would be within the canvas. If not, quit.
			if (this.positionIsWithinCanvas(marker, x, y, pointer))
				markerPositions.push(this.createMarkerPosition(marker, x, y));
			else
				return;
		}

		this.moveMarkers(markerPositions);
	}

	normalizeCoords(coords)
	{
		let boundingInfo = this.calculatePolygonBoundingRectangle(coords);

		let deltaX = boundingInfo.minX;
		let deltaY = boundingInfo.minY;

		if (!(deltaX === 0 && deltaY === 0))
		{
			for (let i = 0; i < coords.length; i += 2)
			{
				coords[i] = coords[i] - deltaX;
				coords[i + 1] = coords[i + 1] - deltaY;
			}
		}

		return { coords, size: boundingInfo.size, deltaX, deltaY };
	}

	nudgeHandle(distance, direction)
	{
		//console.log(`MapEditor::nudgeHandle ${distance} ${direction}`);

		this.nudgingHandle = true;

		const marker = this.markerBeingEdited;

		let deltaX = direction === 'h' ? distance : 0;
		let deltaY = direction === 'v' ? distance : 0;

		if (this.markerBeingEdited.shapeHasPoints)
		{
			// Nudge each selected handle one at a time the same as if the user had dragged
			// them one at a time. See the comments for removeHandle to understand why it's
			// necessary to do it this way rather then nudging them all at once.
			this.excludeEditFromHistory = true;
			for (const index of marker.handles.selected)
				this.applyEditToHandle(deltaX, deltaY, index);
			this.excludeEditFromHistory = false;
		}
		else
		{
			// Nudge a single handle for a rectangle or circle.
			this.applyEditToHandle(deltaX, deltaY, this.selectedMarkers.primaryMarker.handles.selectedHandle.index);
		}

		// Record the nudge of all the selected handles as though it were a single edit.
		this.addEditToHistory(new MarkerEdit(marker));
		this.updateControls();
	}

	nudgeMarkers(distance, direction)
	{
		// Verify that all selected markers can be nudged. If one cannot, don't move any of them.
		let markerPositions = [];
		for (const viewId of this.selectedMarkers.viewIds) 
		{
			let marker = this.map.getMarker(viewId);

			if (marker.isLocked)
				continue;

			let x;
			let y;

			if (direction === 'h')
			{
				x = marker.x + distance;
				y = marker.y;
			}
			else
			{
				x = marker.x;
				y = marker.y + distance;
			}

			if (this.positionIsWithinCanvas(marker, x, y))
				markerPositions.push(this.createMarkerPosition(marker, x, y));
			else
				return;
		}

		// Check to see if an markers were nudged.
		if (markerPositions.length === 0)
			return;

		if (this.editingMarker)
			this.markerBeingEdited.handles.invalidateHandles();

		this.moveMarkers(markerPositions);
	}

	get nudging()
	{
		// Determine if a sequence of nudge actions are being performed.
		let nudging =
			this.previousActionId === this.ACTION_NUDGE_LEFT ||
			this.previousActionId === this.ACTION_NUDGE_UP ||
			this.previousActionId === this.ACTION_NUDGE_DOWN ||
			this.previousActionId === this.ACTION_NUDGE_RIGHT;
		return nudging;
	}

	get numberOfShapeMarkersSelected()
	{
		let count = 0;
		for (const viewId of this.selectedMarkers.viewIds)
		{
			if (this.map.getMarker(viewId).isShapeMarker)
				count += 1;
		}
		return count;
	}

	get oneShapeMarkerSelected()
	{
		if (this.selectedMarkers.count !== 1)
			return false;
		let marker = this.selectedMarkers.primaryMarker;
		return marker.isShapeMarker;

	}

	get okToNudgeHandle()
	{
		let ok = this.editingMarker && this.markerBeingEdited.handles.selectedHandle !== null;
		return ok;
	}

	okToRemoveHandle(marker)
	{
		let minHandles = marker.shapeType === marker.SHAPE_TYPE_LINE ? 3 : 4;
		let ok =
			marker.handles.selectedHandleCount === 1 &&
			marker.handles.count >= minHandles &&
			marker.shapeHasPoints;
		return ok;
	}

	get panningIsDisabled()
	{
		// Disable panning when the pointer is over the marker being edited. This avoids the problem
		// of attempting to drag a handle when the mouse is slightly off of it and so the map pans instead.
		if (this.editingMarker && this.viewIdForMarkerUnderPointer === this.markerBeingEdited.viewId)
			return true;

		return false;
	}

	performAction(actionId)
	{
		// Record the action that is about to be performed. Methods that begin with 'action' should
		// only be called via this performAction method, otherwise the action won't get recorded.
		this.previousActionId = this.currentActionId;
		this.currentActionId = actionId;

		switch (actionId)
		{
			case this.ACTION_TOGGLE_SOFT_CONTROL_KEY:
				this.actionToggleSoftControlKey();
				break;

			case this.ACTION_SELECT_ALL_MARKERS:
				this.actionSelectAllMarkers();
				break;

			case this.ACTION_CHANGE_STACK_ORDER:
				this.actionChangeStackOrder();
				break;

			case this.ACTION_ALLOW_OVER_ZOOM:
				this.actionAllowOverZoom();
				break;

			case this.ACTION_ROTATE_LEFT:
				this.actionRotate(-1);
				break;

			case this.ACTION_ROTATE_RIGHT:
				this.actionRotate(1);
				break;

			case this.ACTION_NUDGE_LEFT:
				this.actionNudge(-1, 'h');
				break;

			case this.ACTION_NUDGE_UP:
				this.actionNudge(-1, 'v');
				break;

			case this.ACTION_NUDGE_RIGHT:
				this.actionNudge(1, 'v');
				break;

			case this.ACTION_NUDGE_DOWN:
				this.actionNudge(1, 'h');
				break;

			case this.ACTION_ALIGN_COLUMN:
				this.actionAlign('v');
				break;

			case this.ACTION_ALIGN_ROW:
				this.actionAlign('h');
				break;

			case this.ACTION_TOGGLE_APPEARANCE:
				this.actionToggleAppearance();
				break;

			case this.ACTION_TOGGLE_TRANSPARENCY:
				this.actionToggleTransparency();
				break;

			case this.ACTION_TOGGLE_LOCK_MARKER:
				this.actionToggleLockMarker();
				break;

			case this.ACTION_GO_TO_EDIT_SCREEN:
				this.actionGoToEditScreen();
				break;

			case this.ACTION_REMOVE:
				this.actionRemove();
				break;

			case this.ACTION_SET_MAP_FOCUS:
				this.actionSetMapFocus();
				break;

			case this.ACTION_SHOW_MAP_FOCUS:
				this.actionShowMapFocus();
				break;

			case this.ACTION_REPLACE_MARKER:
				this.actionReplaceMarker();
				break;

			case this.ACTION_REPLACE_MARKER_STYLE:
				this.actionReplaceMarkerStyle();
				break;

			case this.ACTION_ADD_NEW_HOTSPOT:
				this.actionAddNewHotspot();
				break;

			case this.ACTION_CREATE_NEW_MARKER:
				this.actionCreateNewMarker();
				break;

			case this.ACTION_DUPLICATE_MARKER:
				this.actionDuplicateMarker();
				break;

			case this.ACTION_EDIT_MARKER:
				this.actionEditMarker();
				break;

			case this.ACTION_CONVERT_HYBRID:
				this.actionConvertHybrid();
				break;

			case this.ACTION_FINISH_EDITING:
				this.actionFinishEditing();
				break;

			case this.ACTION_UNDO_EDIT:
				this.actionUndoEdit();
				break;

			case this.ACTION_REDO_EDIT:
				this.actionRedoEdit();
				break;

			case this.ACTION_CANCEL_EDITING:
				this.actionCancelEditing();
				break;
		}
	}

	performDrag(deltaX, deltaY, pointer)
	{
		if (deltaX === 0 && deltaY === 0)
			return;

		//console.log(`MapEditor::performDrag ${deltaX},${deltaY}`);

		// Show the appropriate dragging cursor for what is being dragged.
		this.setCursor();

		if (this.draggingMarker)
		{
			// Translate the movement to the map's scale.
			let marker = this.map.getMarker(this.viewIdForMarkerBeingDragged);
			if (marker.isAnchored)
			{
				deltaX = this.map.convertScreenToMap(deltaX);
				deltaY = this.map.convertScreenToMap(deltaY);
			}
		}
		else if (this.draggingCrosshairs)
		{
			//console.log(`MapEditor::performDrag CROSSHAIRS ${deltaX},${deltaY}`);
			if (this.crosshairRectUnderPointer === this.crosshairRectH ||
				this.crosshairRectUnderPointer === this.crosshairRectV)
			{
				// Allow dragging in both directions regardless of which crosshair is being dragged.
				deltaX = this.map.convertScreenToMap(deltaX);
				deltaY = this.map.convertScreenToMap(deltaY);
			}
		}

		if (this.draggingHandle)
		{
			let handles = this.selectedMarkers.primaryMarker.handles;
			let index = handles.handleBeingDragged.index;
			this.applyEditToHandle(deltaX, deltaY, index, pointer);
		}
		else
		{
			// The logic above is for the primary selected marker (the one the crosshairs go through),
			// but other markers might also be selected. Update all of their positions.
			this.moveSelectedMarkers(deltaX, deltaY, pointer);
		}
	}

	performDragOnEachAnimationFrame()
	{
		if (!(this.draggingMarker || this.draggingCrosshairs || this.draggingHandle))
			return;

		//console.log(`MapEditor::performDragOnEachAnimationFrame ${this.dragDeltaX},${this.dragDeltaY}`);

		// Drag markers or crosshairs by the most recently calculated distance since the last drag.
		this.performDrag(this.dragDeltaX, this.dragDeltaY, this.currentPointerLocation);

		// Remember the current drag location for use in computing the next drag deltas.
		this.lastDragLocation = this.currentPointerLocation;

		// Reset the deltas to zero so that no dragging will occur if the user pauses while dragging
		// the mouse or their finger as this performDragOnEachAnimationFrame is continuing to call itself.
		this.dragDeltaX = 0;
		this.dragDeltaY = 0;

		requestAnimationFrame(this.performDragOnEachAnimationFrame);
	}

	placeMarkerOnMap(marker)
	{
		console.log(`MapEditor::placeMarkerOnMap`);

		// This method is called when the user selects the hotspot for a marker that is not currently
		// positioned on the map. They do that either by choosing from the hotspot list above the map
		// or clicking on a thumbnail below the map. This method initially displays the marker at the top
		// of the map, horizontally centered, and then initiates an animation that gradually moves the
		// marker down until it is vertically centered. The animation is done asynchronously by the 
		// placeMarkerOnMapAnimation method which this method calls. That call returns immediately, and so
		// this method returns to its caller immediately even though the animation is still occurring.

		// Set the marker's initial screen position to the top center of the canvas, or if the map is
		// scaled so that it doesn't occupy the full width of the canvas, position to the center of the map.
		let w = Math.min(this.map.editLayer.width, this.map.convertMapToScreen(this.map.mapW_actual));
		let x = Math.round(w / 2);
		let y = 0;

		// Adjust the position by the amount the map is panned in screen pixels.
		let panX_screen = marker.isAnchored ? this.map.convertMapToScreen(this.map.panX_map) : 0;
		let panY_screen = marker.isAnchored ? this.map.convertMapToScreen(this.map.panY_map) : 0;
		x -= panX_screen;
		y = panY_screen;

		// Convert the screen position to the map position.
		x = this.map.convertScreenToMap(x);
		y = this.map.convertScreenToMap(y);

		// Place the marker at its map position.
		this.updateMarkerPosition(marker, x, y);

		// Record which marker instance is being animated.
		this.animatedMarker = marker;

		// Begin the animation.
		requestAnimationFrame(this.placeMarkerOnMapAnimation);
	}

	placeMarkerOnMapAnimation()
	{
		// See comments for the placeMarkerOnMap method above for an explanation of how these methods work together.

		// Determine what the y location of the marker should be when it is at its final resting place.
		let unscaledHeight = Math.round((this.map.editLayer.height / 2) / this.map.currentMapScale);
		let yTarget = unscaledHeight;

		// Adjust the target location for panning.
		if (this.animatedMarker.isAnchored)
			yTarget -= this.map.panY_map;

		// Get the current y location of the marker.
		let y = this.animatedMarker.y;

		// Quit if the marker is in its final position.
		if (y >= yTarget)
		{
			this.animatedMarker = null;
			this.map.reportMarkerCoords();
			this.reportMarkersOnMap();

			// Redraw the map and the placed marker to ensure that the marker is showing as selected
			// on the high resolution map image. This is necessary to handle the case where the cached
			// marker layer that was used during the animation was for a low resolution map image. This
			// can occur because when you place a new marker on the map, the page reloads and the
			// animation begins immediately, possibly before the high resolution map image has loaded.
			// When that happens, the animation occurs on top of the low resolution image, but once
			// control gets to this code, the map gets redrawn with the high resolution image.
			this.map.flushMarkerLayerCache();
			this.map.drawMap();
			this.redrawSelectedMarkers();
			return;
		}

		// Determine how much further the marker needs to descend.
		let delta = yTarget - y;

		// Calculate the distance the marker will drop during the current animation frame. The distance
		// is 5 % of the remaining distance plus a small constant. During early animation frames, 5% is
		// a larger number than in later frames. This gradual decrease causes the marker to drop quickly
		// in the beginning and more slowly along the way.
		y += Math.round(delta * 0.05);

		// Add a small constant to prevent the drop during the last frames from being too slow.
		y += 5;

		//  Move the marker to its new position.
		this.updateMarkerPosition(this.animatedMarker, this.animatedMarker.x, y);

		if (this.map.markerLayerCacheIsStale)
			this.map.drawAllMarkers();
		else
			this.map.drawCachedMarkerLayer();

		// Draw the marker in it's new position.
		this.map.drawMarkerOnly(this.animatedMarker, true);

		// Request that this method be called back on the next animation frame.
		requestAnimationFrame(this.placeMarkerOnMapAnimation);
	}

	positionIsWithinCanvas(marker, x, y, pointer = null)
	{
		// This method determines if the marker's center position (x,y) is so far left,
		// right, top, or bottom, that nearly the entire marker would be off the canvas.

		let usePointerPosition = pointer !== null;

		if (usePointerPosition)
		{
			if (pointer.x < 0 || pointer.x > this.map.canvasW || pointer.y < 0 || pointer.y > this.map.canvasH)
				return false;
		}

		if (marker.isAnchored)
		{
			x += this.map.panX_map;
			y += this.map.panY_map;
			x = this.map.convertMapToScreen(x);
			y = this.map.convertMapToScreen(y);
		}

		const halfW = this.map.convertMapToScreen(marker.shapeW / 2);
		const halfH = this.map.convertMapToScreen(marker.shapeH / 2);

		if (!usePointerPosition && (x <= -halfW || y <= -halfH))
			return false;

		let w = this.map.canvasW;
		let h = this.map.canvasH;

		if (marker.isAnchored)
		{
			// Handle the case where the canvas is larger than the map at the current
			// zoom level to prevent markers from being positioned off the map itself.
			let scaledMapWidth = this.map.convertMapToScreen(this.map.mapW_actual);
			let scaledMapHeight = this.map.convertMapToScreen(this.map.mapH_actual);
			if (w > scaledMapWidth)
				w = scaledMapWidth;
			if (h > scaledMapHeight)
				h = scaledMapHeight;
		}

		//console.log(`MapEditor:: positionIsWithinCanvas ${x},${y} ${w}x${h} ${halfW}x${halfH}`);

		if (x >= w + halfW || y >= h + halfH)
			return false;

		return true;
	}

	post(hiddenFieldId, value)
	{
		let fieldElement = maGetElementByPageId(hiddenFieldId);
		fieldElement.value = value;
		maChangeDetected();
		maOnEventSave();
	}

	postAddNewHotspot(formData)
	{
		// Store information about the new hotspot in the hidden NewHotspot field and post it back to the server.
		// The information is in a comma semicolon list, so remove any semicolon from the string data.
        let hotspotId = formData.hotspotId.replace(/;/g, '');
        let hotspotTitle = formData.hotspotTitle.replace(/;/g, '');
        let markerId = formData.markerId;
		this.post("NewHotspot", `${markerId};${hotspotId};${hotspotTitle}`);
	}

	postConvertHybrid(marker)
	{
		this.post("HybridConvert", marker.viewId);
	}

	postCreateNewMarker(formData, duplicate, newHotspot)
	{
		let markerAction = "";
		let hotspotAction = "";

		if (!duplicate && newHotspot)
		{
			markerAction = "new";
			hotspotAction = "new";
		}
		else if (!duplicate && !newHotspot)
		{
			markerAction = "new";
			hotspotAction = "existing";
		}
		else if (duplicate && newHotspot)
		{
			markerAction = "duplicate";
			hotspotAction = "new";
		}
		else if (duplicate && !newHotspot)
		{
			markerAction = "duplicate";
			hotspotAction = "existing";
		}

		// Store information about the new hotspot in the hidden NewHotspot field and post it back to the server.
		// The information is in a semicolon separated list, so remove any semicolon from the string data.
		let markerName = formData.markerName.trim().replace(/;/g, '');

		let hotspotData;
		if (hotspotAction === "new")
		{
			let id = formData.hotspotId.trim().replace(/;/g, '');
			let title = formData.hotspotTitle.trim().replace(/;/g, '');
			hotspotData = `${id},${title}`;
		}
		else
		{
			hotspotData = formData.hotspotViewId;
		}

		let marker = this.selectedMarkers.primaryMarker;
		let markerId = duplicate ? marker.markerId : 0;
		let markerStyle = duplicate ? marker.styleId : formData.markerStyleId;
		let shape = duplicate ? this.shapeName(marker) : formData.shape;
		this.post("NewShapeMarker", `${markerAction};${hotspotAction};${markerName};${hotspotData};${shape};${markerId};${markerStyle};${newShapeSideLength}`);
	}

    postReplaceMarkers(formData) {
		// Store the changed markers in the hidden replacedMarkers field and post it back to the server.
		this.post("ReplacedMarkers", `${formData.markerId};${this.selectedMarkers.viewIds.join(',')}`);
    }

    postReplaceMarkerStyles(formData) {
		// Store the changed marker styles in the hidden replacedMarkerStyles field and post it back to the server.
		this.post("ReplacedMarkerStyles", `${formData.markerStyleId};${this.selectedMarkers.viewIds.join(',')}`);
    }

	get primarySelectedMarkerViewId()
	{
		if (this.selectedMarkers.count === 0)
			return 0;
		else
			return this.selectedMarkers.viewIds[0];
	}

	async recordMapState({ updateAfterPanZoom = false, applyFocus = false, pointer = null, mapFocus = false, setFocusPoint = false } = {})
	{
		// This method gets called to record the current Map Editor zoom and pan position whenever the user pans
		// or zooms the map. It also gets called in response to a user request to focus the map. It even gets
		// called recursivly via one level of indirection when the user confirms that they want to focus the map
		// and they don't also need to choose a focus point. In those cases, after confirmation, this method
		// gets called with the applyFocus option to perform the focusing that the user just approved. When the user
		// does need to choose a focus point, this method gets called again, also with the applyFocus option, after
		// the user has clicked the focus point. Because there are so many flavors of map focusing, this method's
		// primary purpose is to direct a focus request to the appropriate method so that callers don't need to know
		// about specific focusing methods. All the methods ultimately call recordMapStateForServer to set the
		// MapEditor zoom/pan settings plus the focus zoom/focus-point settings.

		// Convert the current map scale to percent, but use 0 to indicate that the map is zoomed all the way out.
		// If the map is over-zoomed, record the zoom level as 100%.
		let isZoomedOut = this.map.currentMapScale === this.map.zoomedOutMapScale;
		let mapEditorPercent = this.map.currentMapScale * 100;
		let zoomPercent = isZoomedOut ? 0 : Math.round(Math.min(mapEditorPercent, 100));

		let flexFocus = this.map.tour.isFlexMapTour && mapFocus && isZoomedOut && setFocusPoint;

		// When this method is called to ask the user to confirm that they want to focus the map,
		// change the map focus icon to show its focusing mode apearance.
		this.setMapFocusIcon(mapFocus || flexFocus);

		if (updateAfterPanZoom)
		{
			// The user did not ask to focus the map, but they panned or zoomed the map. Record the current Map Editor
			// state so that the the next time the user goes to the Map Editor, the map's zoom level and position will
			// be the same way it was when they last edited the map.
			this.recordMapStateForServer(mapEditorPercent, true);

			// Exit the map focusing state in case the user panned or zoomed
			// the map while they were supposed to be choosing the focus point.
			this.setMapFocusingState(false);

			return;
		}

		if (setFocusPoint)
		{
			this.turnOffSoftControlKey();

			if (flexFocus)
			{
				// The user wants to flex-focus the map which means setting its focus point.
				await this.recordMapStateAfterConfirmingApplyFocus(-1, true, true);
				return;
			}

			if (mapFocus)
			{
				// The user wants to focus the map at the current zoom level and set the focus point
				await this.recordMapStateAfterConfirmingApplyFocus(zoomPercent, isZoomedOut, true);
				return;
			}
		}
		else
		{
			if (mapFocus)
			{
				// The user wants to focus the map at the current zoom level without settomg the focus point
				await this.recordMapStateAfterConfirmingApplyFocus(zoomPercent, isZoomedOut, false);
				return;
			}
		}

		if (applyFocus)
		{
			// The user wants to focus the map using the default focus point (mapFocus), or they previously
			// chose to set the focus point and just now clicked on the focus point (applyFocus).
			let useDefaultFocusPoint = pointer === null;
			this.recordMapStateToApplyFocus(mapEditorPercent, zoomPercent, pointer, useDefaultFocusPoint);
			return;
		}

	}

	async recordMapStateAfterConfirmingApplyFocus(zoomPercent, isZoomedOut, setFocusPoint)
	{
		let chooseFocusPoint;
		let applyFocusWithNoFocusPoint;
		let instructions;
		let learn = `[Learn about <a href="https://mapsalive.com/docs/start-map-editor/#map-focus" target="_blank">map focus</a>.]`;
		let steps = `<ul><li>First press CONTINUE</li><li>Then click where you want the focus point</li></ul>`;
		steps += learn;

		if (this.map.tour.enableV3Compatibility)
		{
			// Determine whether the map should be focused zoomed all the way in or all the way out based on
			// whether the percentage is below or above the point where the map is zoomed half way.
			let focusZoomedOut = zoomPercent <= this.map.mapZoomMidLevelV3;

			instructions = `Set the map focus to zoomed ${focusZoomedOut ? 'all the way out' : 'to 100% at its current position.'}`;
			instructions = `[${instructions}?]${this.cancelInstructions}`;
			applyFocusWithNoFocusPoint = await maAwaitConfirm(instructions, 'SET');
			chooseFocusPoint = false;
		}
		else if (zoomPercent === -1)
		{
			instructions = `Set this Flex Map to automatically fill the browser`;
			instructions = `[${instructions}:${steps}]${this.cancelInstructions}`
			applyFocusWithNoFocusPoint = false;
			chooseFocusPoint = await maAwaitConfirm(instructions, 'CONTINUE');
		}
		else if (isZoomedOut)
		{
			instructions = `[Set the map focus to zoomed all the way out.]${this.cancelInstructions}`;
			instructions += learn;
			applyFocusWithNoFocusPoint = await maAwaitConfirm(instructions, 'SET');
			chooseFocusPoint = false;
		}
		else
		{
			if (setFocusPoint)
			{
				instructions = `Set the map focus to the current zoom level and choose the focus point`;
				instructions += `:${steps}`;
			}
			else
			{
				let amount = zoomPercent >= 100 ? "zoom level to 100%" : " to the current zoom level";
				instructions = `[Set the map focus ${amount}.]`;
				instructions += learn;
			}
			instructions = `[${instructions}${this.cancelInstructions}`
			let ok = await maAwaitConfirm(instructions, setFocusPoint ? 'CONTINUE' : 'SET');
			applyFocusWithNoFocusPoint = ok && !setFocusPoint;
			chooseFocusPoint = ok && setFocusPoint;
		}

		if (applyFocusWithNoFocusPoint)
		{
			// The user confirmed that they want to focus the map in a way that no focus
			// point is required. Call recordMapState recursively to perform the focusing.
			this.inMapFocusingState = true;
			this.recordMapState({ applyFocus: true });
			return;
		}

		if (chooseFocusPoint)
		{
			// The user confirmed that they want to focus the map in a way that requires a focus point.
			// Display the map in its focusing mode where it is faded and the cursor is a cross. After
			// the user clicks the point, recordMapState will get called with the applyFocus option.
			this.showMessage(`Click the focus point. Press Esc to cancel.`, true);
			this.setMapFocusingState(true, zoomPercent === -1);
			return;
		}

		// The user pressed Cancel. Restore the focus icon to its normal appearance.
		this.setMapFocusIcon(false);
	}

	recordMapStateForServer(mapEditorPercent, recordState)
	{
		if (this.map.tour.enableV3Compatibility && !recordState)
		{
			// Don't update the state for a V3 tour because there is no way to distinguish between the last
			// Map Editor state and the map focus state. Because the state is not recorded, every time the user
			// goes to the Map Editor, they see the map in the focus state instead of how it was when they 
			// they last edited the map. This could possibly be addressed by using the V4 focus x,y values
			// to record the Map Editor pan x,y for a V3 tour, and then resetting them if the user converts
			// to V4, but for now, users who choose to stick with V3 will be inconvenienced.
			return;
		}

		// Update the server-side focus state by updating the "ZoomState" hidden field in the Map.aspx page.
		// It will get posted back to the server when the user transfers to another page or saves the Map
		// page. Map.aspx.cs will then extract the map state values from the ZoomState field.
		let e = maGetElementByPageId("ZoomState");
		let oldState = e.value;
		e.value = `${mapEditorPercent},${this.map.panX_map},${this.map.panY_map},${this.map.mapFocusX},${this.map.mapFocusY},${this.map.mapFocusPercent}`;
		//console.log(`MapEditor::recordMapStateForServer ${e.value}`);

		// Enable the Save/Undo buttons. A future version of the Map Editor could use AJAX calls to send these
		// settings to the server, but for now, if the user quits the browser without saving, or going to another
		// Tour Builder page, the state recorded here will be lost.
		if (recordState || oldState != e.value)
			maChangeDetected();
	}

	recordMapStateToApplyFocus(mapEditorPercent, zoomPercent, pointer, useDefaultFocusPoint)
	{
		// Override the zoom percent to be -1 when setting a flex focus.
		if (this.settingFlexFocus)
			zoomPercent = -1;

		this.setMapFocusingState(false);

		if (this.map.tour.enableV3Compatibility)
		{
			// V3 maps don't have a focus point so set the focus x,y to 0,0.
			this.saveZoomAndFocusSettings(0, 0, zoomPercent);
		}
		else if (pointer === null && !useDefaultFocusPoint)
		{
			// The pointer will be null when this method is called recursively to focus a map
			// zoomed all the way out because in that case, there is no focus point.
			this.saveZoomAndFocusSettings(0, 0, 0);
		}
		else
		{
			if (useDefaultFocusPoint)
			{
				// Choose the focus point as though the user had clicked on the
				// center of the visible portion of the map withing the map editor.
				let scaledMapWidth = this.map.convertMapToScreen(this.map.mapW_actual);
				let scaledMapHeight = this.map.convertMapToScreen(this.map.mapH_actual);
				let w = Math.min(this.map.mapAreaW, scaledMapWidth);
				let h = Math.min(this.map.mapAreaH, scaledMapHeight);
				let x = Math.round(w / 2);
				let y = Math.round(h / 2);
				pointer = { x, y };
			}

			let mapFocusX = -this.map.panX_map + this.map.convertScreenToMap(pointer.x);
			let mapFocusY = -this.map.panY_map + this.map.convertScreenToMap(pointer.y);

			this.saveZoomAndFocusSettings(mapFocusX, mapFocusY, zoomPercent);

			// Show the focus point in crosshairs unless focused zoomed out using the default focus
			// point (as opposed to zoomed out in order to set the focus point for Flex Focus)
			if (!(zoomPercent === 0 && useDefaultFocusPoint))
				this.performAction(this.ACTION_SHOW_MAP_FOCUS);
		}

		this.showFocusStateInformation();
		this.recordMapStateForServer(mapEditorPercent, true);
	}

	redrawHybrid()
	{
		//console.log(`MapEditor::redrawHybrid`);
		this.hybridMarkerBeingEdited.updateBounds();
		this.eraseEditLayer();
		this.drawOutlineForMarker(this.hybridMarkerBeingEdited);
	}

	redrawSelectedMarkers()
	{
		//console.log(`MapEditor::redrawSelectedMarkers ${this.selectedMarkers.count}`);

		// Erase the existing outlines and crosshairs. This is necessary even when this method is
		// called when no markers are selected as happens when you ctrl-click the only selected
		// marker in order to deslect it.
		this.eraseEditLayer();

		// Redraw all the selected markers.
		for (const viewId of this.selectedMarkers.viewIds)
		{
			let marker = this.map.getMarker(viewId);
			marker.appearsSelected = true;

			if (marker.typeIsSymbol)
				this.map.drawMarkerAndNeighbors(marker, true);
			else
				this.map.drawMarkerOnly(marker, true);

			this.drawOutlineForMarker(marker);
		}
	}

	removeHandle()
	{
		// Only one handle can be deleted at a time because the logic to support deletion of
		// multiple handles would require significant effort for a little used operation.
		// As explained in the comment below, deleting even one handle is tricky, but once a
		// handle is deleted, all of the remaining handles need to get recreated. Some will
		// get a new index and/or new prev/next values, and some may get new x/y and coord
		// values due to normalization. As such, it's not possible to simply loop over the
		// selected handles to delete them one at a time, because as soon as one gets deleted,
		// the iteration is no longer valid. A more sophisticated approach would be required.

		let handles = this.markerBeingEdited.handles;
		let selectedHandle = handles.selectedHandle;

		// Move the handle to be deleted to the point between its adjacent handles. This causes the
		// polygon's bounding box to get resized, its coords normalized, and its center point shifted
		// just as if the user had dragged the handle to that point. Then the handle can be deleted
		// without affecting the polygon's size, center, or its other coords.
		let prev = selectedHandle.prev;
		let next = selectedHandle.next;
		let newX = Math.round((prev.x + next.x) / 2);
		let newY = Math.round((prev.y + next.y) / 2);
		let deltaX = this.map.convertScreenToMap(newX - selectedHandle.x);
		let deltaY = this.map.convertScreenToMap(newY - selectedHandle.y);

		this.excludeEditFromHistory = true;
		this.applyEditToHandle(deltaX, deltaY);
		handles.deleteHandle();
		this.excludeEditFromHistory = false;

		this.applyEditToHandle(0, 0);
		handles.initialize();

		//	// Deleting a handle cannot be undone clear the edit history.
		//	this.clearEditHistory();
		//	this.updateControls();
	}

	removeMarker()
	{
		// Use a copy of the view Ids array that won't be side-affected by removal of markers from the actual array.
		let viewIds = [...this.selectedMarkers.viewIds];

		let markersRemoveCount = 0;
		for (let viewId of viewIds)
		{ 
			let marker = this.map.getMarker(viewId);

			console.log(`REMOVE ${viewId}`)

			if (marker.isLocked)
			{
				if (this.selectedMarkers.count === 1)
					maAlert("[The marker cannot be removed because it is locked.][You can unlock it by clicking the lock icon.]")
				continue;
			}

			this.selectedMarkers.removeMarker(viewId);
			markersRemoveCount += 1;

			// Change the marker's position to be off the map.
			marker.pctX = -0.5;
			marker.pctY = -0.5;
			let x = Math.round(this.map.mapW_actual * marker.pctX);
			let y = Math.round(this.map.mapH_actual * marker.pctY);
			this.updateMarkerPosition(marker, x, y);

			// Refresh the marker's hotspot thumbnail so the red dot appears.
			maShowMarkerThumbAsHidden(viewId);
		}

		// Clear the hotspot dropdown list so that it does not show the removed marker.
		if (markersRemoveCount > 0)
			maShowMarkerInHotspotList(0);

		this.map.reportMarkerCoords();
		this.map.flushMarkerLayerCache();
		this.map.drawMap();
		this.updateControls();
		this.setMarkerLockImage();

		// Hide the crosshairs and the outlines of any markers that were selected.
		this.eraseEditLayer();

		this.reportMarkersOnMap();
	}

	removePseudoMarkerFromHybrid()
	{
		if (this.selectedMarkers.count === this.map.markers.length)
		{
			maAlert("The selected shape(s) cannot be removed because a hybrid marker must contain at least one shape.");
			return;
		}

		// Make a copy of the selected markers array and then deslect all the markers.
		let viewIds = [...this.selectedMarkers.viewIds];
		this.deselectAllMarkers();

		// Loop over all the markers and copy each to the updated markers array except for the ones being removed.
		let updatedMarkers = [];
		for (const marker of this.map.markers)
		{
			let remove = false;

			// See if this marker is one that's being removed.
			for (let viewId of viewIds)
			{
				if (marker.viewId === viewId)
				{
					remove = true;
					break;
				}
			}
			if (!remove)
				updatedMarkers.push(marker);
		}

		// Set the actual markers array with just the markers that are left.
		this.map.markers = updatedMarkers;

		this.map.createHybridMarkerFromPseudoMarkers(this.hybridMarkerBeingEdited);
		this.map.drawMap();

		this.drawOutlineForMarker(this.hybridMarkerBeingEdited);

		this.reportMarkerEdit(this.hybridMarkerBeingEdited);
	}

	reportMarkerEdit(marker)
	{
		if (marker.isPseudoMarker)
			return;

		// Determine if this marker already has unsaved edits.
		let index = 0;
		let found = false;
		for (const markerEdit of this.unsavedMarkerEdits)
		{
			if (markerEdit.markerId === marker.markerId)
			{
				found = true;
				break;
			}
			index += 1;
		}

		// Update or add this marker's edits to the unsaved edits.
		let markerEdit = new MarkerEdit(marker);
		if (found)
			this.unsavedMarkerEdits[index] = markerEdit;
		else
			this.unsavedMarkerEdits.push(markerEdit)

		// Create an array of the data for each marker and then join them together into a vertical bar separated list.
		let allUnsavedEdits = [];
		for (markerEdit of this.unsavedMarkerEdits)
			allUnsavedEdits.push(markerEdit.data);
		let editsString = allUnsavedEdits.join('|');

		// Save the string of edits in the pages hidden field. This way if the 
		// user leaves the page, these latest edits will get posted to the server.
		let fieldElement = maGetElementByPageId("EditedMarkers");
		fieldElement.value = editsString;

		// Update the marker coords that will be posted to the server.
		this.map.reportMarkerCoords();

		//console.log(`MapEditor::reportMarkerEdit ${fieldElement.value}`);
	}

	reportMarkersOnMap()
	{
		let count = 0;
		for (let marker of this.map.markers)
		{
			if (marker.x > 0 && marker.y > 0)
				count += 1;
		}

		maMarkersOnMapChanged(count, this.map.markers.length);
	}

	restoreMarkerEdit(marker, edit)
	{
		// This should never happen, if it does, ignore the call.
		if (!edit)
		{
			debugger;
			return;
		}

		marker.shapeW = edit.shapeW;
		marker.shapeH = edit.shapeH;
		marker.shapeCoords = edit.shapeCoords;
		marker.x = edit.x;
		marker.y = edit.y;
		marker.pctX = edit.pctX;
		marker.pctY = edit.pctY;
		marker.updateBounds();

		this.markerBeingEdited.handles.initialize();

		this.reportMarkerEdit(marker);

		// Redraw the map in case the edits had enlarged the marker outside the bounds of the original.
		this.map.drawMap();
		this.redrawSelectedMarkers();

		if (marker.isPseudoMarker)
		{
			this.map.createHybridMarkerFromPseudoMarkers(this.hybridMarkerBeingEdited);
			this.drawOutlineForMarker(this.hybridMarkerBeingEdited);
		}
	}

	get rotating()
	{
		// Determine if a sequence of rotation actions are being performed.
		let rotating =
			this.previousActionId === this.ACTION_ROTATE_LEFT ||
			this.previousActionId === this.ACTION_ROTATE_RIGHT;
		return rotating;
	}

	saveZoomAndFocusSettings(focusX, focusY, percent)
	{
		this.map.mapFocusX = focusX;
		this.map.mapFocusY = focusY;
		this.map.mapFocusPercent = percent;
	}

	selectAdjacentHandle(prev)
	{
		if (this.markerBeingEdited.handles.selectedHandle === null)
			return;
		this.markerBeingEdited.handles.setAdjacentHandleSelected(prev);
		this.map.drawMarkerOnly(this.markerBeingEdited, true);
	}

	selectedMarkersAreLocked()
	{
		// When at least one of the selected markers is locked, show them all as locked.
		for (const viewId of this.selectedMarkers.viewIds)
		{
			let marker = this.map.getMarker(viewId);
			if (marker.isLocked)
				return true;
		}

		return false;
	}

	setCursor()
	{
		if (this.inMapFocusingState)
			return;

		let cursor = 'auto';
		let marker = this.selectedMarkers.primaryMarker;

		if (marker && marker.handles.handleUnderPointer !== null)
		{
			// The mouse is over a shape marker's edit handle.
			cursor = 'pointer';
		}
		else if (this.viewIdForMarkerWithCrosshairs !== 0 && this.crosshairRectUnderPointer !== null)
		{
			// The mouse is over the crosshairs.
			if (this.viewIdForMarkerUnderPointer === this.viewIdForMarkerWithCrosshairs)
			{
				// The mouse is also over the marker that belongs to the crosshairs.
				cursor = 'pointer';
			}
			else
			{
				if (this.crosshairRectUnderPointer === this.crosshairRectH)
					cursor = 'ns-resize';
				else if (this.crosshairRectUnderPointer === this.crosshairRectV)
					cursor = 'ew-resize';
			}
		}
		else if (this.draggingMarker && this.isMapEditor)
		{
			// Use the pointer instead of the 'move' cursor for dragging a marker because there are too many
			// cases where the 'move' cursor gets overridden by the pointer, e.g. when the pointer is over the
			// crosshairs where they overlap the cursor. By using the pointer, the cursor is consistent.
			cursor = 'pointer';
		}
		else if (this.viewIdForMarkerUnderPointer !== 0)
		{
			// The mouse is over a marker.
			cursor = 'pointer';
		}

		//console.log(`MapEditor::setCursor ${cursor} Crosshairs: ${this.viewIdForMarkerWithCrosshairs} Pointer: ${this.viewIdForMarkerUnderPointer} Dragging marker: ${this.viewIdForMarkerBeingDragged}`);
		this.map.markerLayer.style.cursor = cursor;
	}

	setMapFocusIcon(focusing)
	{
		let suffix = focusing ? 'Active' : '';
		let control = this.getControl(this.ACTION_SET_MAP_FOCUS);
		control.setIcon(`../Images/${control.actionName}${suffix}.png`);
	}

	async setMapFocusingState(focusing, settingFlexFocus = false)
	{
		//console.log(`MapEditor::setMapFocusingState ${focusing} ${settingFlexFocus}`);
		this.setMapFocusIcon(focusing);

		if (!this.map.panningMap)
		{
			this.eraseEditLayer();

			// Don't set the cursor while panning which shows the 'move' cursor.
			this.map.markerLayer.style.cursor = focusing ? 'crosshair' : 'auto';
		}

		this.inMapFocusingState = focusing;
		this.settingFlexFocus = settingFlexFocus;
		if (!focusing)
			this.showMessage("");
	}

	setMarkerHybridImage(isHybrid, enable)
	{
		let suffix = "";
		if (enable && (isHybrid || this.hybridEditingEnabled))
			suffix = this.hybridEditingEnabled ? "Enabled" : "ToShape";
		let control = this.getControl(this.ACTION_CONVERT_HYBRID);
		control.setIcon(`../Images/${control.actionName}${suffix}.png`);
	}

	setMarkerLockImage()
	{
		let suffix = this.selectedMarkersAreLocked() ? 'Lock' : 'Unlock';
		let control = this.getControl(this.ACTION_TOGGLE_LOCK_MARKER);
		control.setIcon(`../Images/Marker${suffix}.png`);
	}

	setMarkerSelected(viewId)
	{
		//console.log(`MapEditor::setMarkerSelected ${viewId}`);

		// Ignore a click on the selected marker.
		if (this.selectedMarkers.primaryViewId === viewId && !this.ctrlKeyIsDown)
			return;

		// Allow a marker to be selected via click-drag when no other markers are selected.
		if (this.draggingMarker && this.selectedMarkers.count > 0)
			return;

		// Don't allow another marker to be selected while editing a marker. This logic used to allow
		// you to click on another shape marker and stay in editing mode, but it was too easy to
		// inadvertantly click on an adjacent marker and have it become the marker being edited.
		if (this.editingMarker)
			return false;

		let add = true;
		let placeMarkerOnMap = false;

		let marker = this.map.getMarker(viewId, false);
		if (marker.isRoute)
			return;

		if (this.ctrlKeyIsDown && this.isMapEditor)
		{
			// Prevent a ctrl-click from deselecting a marker that is being edited.
			if (this.editingMarker)
				return;

			if (this.selectedMarkers.contains(viewId))
			{
				// Deselect the ctrl-clicked marker.
				console.log(`MapEditor::setMarkerSelected REMOVE ${viewId}`);
				this.selectedMarkers.removeMarker(viewId);
				this.deselectMarker(viewId);
				this.redrawSelectedMarkers();
				add = false;
			}
		}
		else
		{
			// Select a marker that is not already selected.
			this.deselectAllMarkers();
		}

		if (add)
		{
			// console.log(`MapEditor::setMarkerSelected ADD ${viewId}`);

			// Determine if the view's marker appears on the gallery map. If the marker's location is
			// negative, it's not in on the map because it doesn't fit. If the view had no marker, that
			// would mean that the marker had not yet been added to the gallery, but that should never
			// happen (marker should never be null) because the Gallery Editor page detects that case and
			// forces the tour, and thus the gallery, to get rebuilt, then reloads the Gallery Editor page
			// so every marker in the Hotspot Order list is in the gallery whether or not it fits on the map.
			if (this.isGalleryEditor)
			{
				if (marker === null || marker.x < 0)
				{
					this.showMessage(`There's no room in the gallery for the marker you selected`);
					return false;
				}
			}

			// Add the selection. If no other markers are selected it becomes the primary marker,
			// otherwise, it becomes a secondary marker.
			this.selectedMarkers.addMarker(viewId);
			maShowMarkerThumbAsSelected(viewId);

			if (this.isMapEditor)
			{
				if (marker.isNotOnMap)
					placeMarkerOnMap = true;
				else
					this.map.positionMapToMakeMarkerVisibleOnCanvas(viewId);
			}
		}

		if (this.isMapEditor)
			this.setMarkerLockImage();

		// Check to see if the map is auto-panning to bring the selected marker into view. If it is,
		// then defer showing the selected marker in the crosshairs until auto-panning completes.
		// When it does complete, the auto-panning logic will call this method again to select the marker.
		if (this.map.autoPanningMap)
			return false;

		// Show the 'Choose' option when zero or more markers are selected.
		if (this.selectedMarkers.count === 1 && marker && !marker.isPseudoMarker)
			maShowMarkerInHotspotList(viewId);
		else
			maShowMarkerInHotspotList(0);

		// Draw the selected marker. If it has not been placed on the map yet, this will cause the marker
		// cache to get created with the marker not showing. This way the cache can be used during the
		// animation that drops the marker onto the map.
		this.redrawSelectedMarkers();

		// Initiate the animation that will drop the marker onto the map. Before starting the animation,
		// force the marker layer cache to get created with no markers selected. The cache will be used
		// for the duration of the animation so that the other markers don't need to be drawn over and
		// over. The force parameter ensures that the cache will get created even if the high resolution
		// map image has not yet loaded. Also see comments in placeMarkerOnMapAnimation(.)
		if (placeMarkerOnMap)
		{
			this.deselectAllMarkers();
			const force = true;
			this.map.createMarkerLayerCache(force);
			this.selectedMarkers.addMarker(marker.viewId);
			maShowMarkerThumbAsSelected(marker.viewId);
			this.placeMarkerOnMap(marker);
		}

		return true;
	}

	setMessageAreaStyle()
	{
		if (this.showingNotificationMessage)
			this.mapControlsMessageArea.classList.add('maNotification');

		else
			this.mapControlsMessageArea.classList.remove('maNotification');
	}

	shapeName(marker)
	{
		switch (marker.shapeType)
		{
			case marker.SHAPE_TYPE_CIRCLE:
				return "circle";

			case marker.SHAPE_TYPE_POLYGON:
				return "polygon";

			case marker.SHAPE_TYPE_LINE:
				return "line";

			case marker.SHAPE_TYPE_HYBRID:
				return "hybrid"

			case marker.SHAPE_TYPE_RECTANGLE:
				return "rectangle";
		}
	}

	shapeType(marker, shapeName)
	{
		switch (shapeName)
		{
			case "circle":
				return marker.SHAPE_TYPE_CIRCLE;

			case "polygon":
				return marker.SHAPE_TYPE_POLYGON;

			case "line":
				return marker.SHAPE_TYPE_LINE;

			case  "rectangle":
				return marker.SHAPE_TYPE_RECTANGLE;
		}
	}

	async showDialogAddNewHotspot()
	{
		let formHtml = "";
		formHtml += this.createHtmlForHotspotFields();

		formHtml += "<div style='margin-top:12px;' class='vex-dialog-message'>Choose a marker for the hotspot:</div>";

		formHtml += this.createHtmlForMarkerSelectList();

		let message = "Add a new hotspot to this map";
		let formData = await maAwaitDialog(message, formHtml, 'ADD HOTSPOT');

		if (formData)
			this.postAddNewHotspot(formData);
	}

	async showDialogChangeStackOrder()
	{
		let formHtml = "";
		formHtml += this.createHtmlForStackRadioButtons();
		let message = "Change the stacking order of the selected marker:";
		let formData = await maAwaitDialog(message, formHtml, 'REORDER');

		if (formData)
		{
			const above = formData.stackChoice === "above";
			this.map.changeMarkerStackingOrder(this.selectedMarkers.primaryMarker.viewId, above);

			// Deselect the marker so it will display in it's stacking order instead of on top because it's selected.
			this.deselectAllMarkers();
		}
	}

	async showDialogCreateNewMarker(duplicate)
	{
		let formHtml = "";

		if (!duplicate)
		{
			let lastShape = maGetElementByPageId("LastShape").value;
			formHtml += this.createHtmlForShapeRadioButtons(lastShape);
			formHtml += "<hr/>";
		}

		formHtml += this.createHtmlForMarkerNameField(duplicate);

		formHtml += "<hr/>";
		formHtml += this.createHtmlForHotspotRadioButtons(duplicate);

		let lastChoice = maGetElementByPageId("LastHotspotChoice").value;
		let useExistingHostpot = lastChoice === "existing";

		formHtml += "<hr/>";
		formHtml += this.createHtmlForHotspotFields(useExistingHostpot);

		// Show the select list unless this map has no hotspots.
		if (hotspotData.length > 0)
			formHtml += this.createHtmlForHotspotSelect(useExistingHostpot);

		if (!duplicate)
		{
			formHtml += "<hr/>";
			formHtml += "<div style='margin-top:12px;' class='vex-dialog-message'>Choose the marker's style:</div>";
			formHtml += this.createHtmlForMarkerStyleSelectList();
		}

		let message = duplicate ? "Create a duplicate of the selected marker named:" : `Create a new shape marker:`;
		let primaryButton = duplicate ? "DUPLICATE MARKER" : "CREATE MARKER";
		let formData = await maAwaitDialog(message, formHtml, primaryButton);

		if (formData)
			this.postCreateNewMarker(formData, duplicate, formData.hotspotChoice === "new");
	}

	async showDialogCreateNewPseudoMarker()
	{
		let formHtml = "";
		formHtml += this.createHtmlForShapeRadioButtons(this.lastShape);

		let message = `Add a shape to the hybrid marker`;
		let primaryButton = "ADD SHAPE";
		let formData = await maAwaitDialog(message, formHtml, primaryButton);

		if (formData)
		{
			this.addPseudoMarkerToHybrid(formData.shape);
			this.lastShape = formData.shape;
		}
	}

	async showDialogGoToEditScreen()
	{
		let marker = this.selectedMarkers.primaryMarker;

		let formHtml = "";

		formHtml += `
			<input type="radio" checked id="editHotspot" name="editChoice" value="hotspot"><label class="map-editor-dialog-radio" for="editHotspot">Edit Hotspot Content</label><br>
			<input type="radio" id="hotspotAdvanced" name="editChoice" value="hotspotAdvanced"><label class="map-editor-dialog-radio" for="hotspotAdvanced">Hotspot Advanced Options</label><br>
			<input type="radio" id="editMarker" name="editChoice" value="marker"><label class="map-editor-dialog-radio" for="editMarker">Edit Marker</label><br>
			<input type="radio" id="markerActions" name="editChoice" value="markerActions"><label class="map-editor-dialog-radio" for="markerActions">Marker Actions</label>`;

		if (!marker.typeIsSymbol)
			formHtml += `
				<br><input type="radio" id="editMarkerStyle" name="editChoice" value="style"><label class="map-editor-dialog-radio" for="editMarkerStyle">Edit Marker Style</label>`;

		let message = "Choose which screen to go to for the selected hotspot:";
		let formData = await maAwaitDialog(message, formHtml, "EDIT");

		if (formData)
		{
			let url = "";
			if (formData.editChoice === "hotspot")
				url = "TourViewEditor.ashx?vid=" + this.selectedMarkers.primaryViewId + "&aid=" + actionIdEditView;
			else if (formData.editChoice === "hotspotAdvanced")
				url = "TourViewEditor.ashx?vid=" + this.selectedMarkers.primaryViewId + "&aid=" + actionIdHotspotAdvanced;
			else if (formData.editChoice === "marker")
				url = "EditMarker.aspx?id=" + marker.markerId;
			else if (formData.editChoice === "markerActions")
				url = "TourViewEditor.ashx?vid=" + this.selectedMarkers.primaryViewId + "&aid=" + actionIdMarkerActions;
			else if (formData.editChoice === "style")
				url = "EditMarkerStyle.aspx?id=" + marker.markerStyle.id;

			maOnEventSaveAndTransfer("/Members/" + url);
		}
	}

	async showDialogReplaceMarker()
	{
		let formHtml = "";
		formHtml += this.createHtmlForMarkerSelectList();

		let message = "Choose a marker to replace the ";
		if (this.selectedMarkers.count === 1)
			message += "selected marker"
		else
			message += `${this.selectedMarkers.count} selected markers`;
		let formData = await maAwaitDialog(message, formHtml, 'REPLACE');

		if (formData)
			this.postReplaceMarkers(formData);
	}

	async showDialogReplaceMarkerStyle()
	{
		let formHtml = this.createHtmlForMarkerStyleSelectList();
		let message = "Choose a style to replace the ";
		if (this.selectedMarkers.count === 1)
			message += "selected marker's style"
		else
			message += `${this.selectedMarkers.count} selected markers styles`;
		let formData = await maAwaitDialog(message, formHtml, 'REPLACE');

		if (formData)
			this.postReplaceMarkerStyles(formData);
	}

	showFocusStateInformation()
	{
		let focusedAtPercent = `focused zoomed to ${this.map.mapFocusPercent}%`;
		let focusIsAt = ` with its focus point at ${this.map.mapFocusX},${this.map.mapFocusY}.`;
		let message = `The map is `;

		if (this.map.mapFocusPercent === -1)
			message += 'flex-focused' + focusIsAt;
		else if (this.map.mapFocusPercent === 0)
			message += `focused zoomed all the way out`;
		else
			message += focusedAtPercent + focusIsAt;

		this.showMessage(message, true);
	}

	showMarkerCoordsMessage(marker)
	{
		if (marker === null)
		{
			this.showZoomPanInformation();
			return
		}

		if (this.selectedMarkers.count === 1)
		{
			this.showMarkerInformation(marker.viewId);
			return;
		}

		let message = '';
		if (this.selectedMarkers.count > 0)
			message = `${this.selectedMarkers.count} markers are selected. Ctrl-click map to deselect all markers.`;

		this.showMessage(message);
	}

	showMarkerInformation(viewId)
	{
		if (viewId !== 0)
		{
			let marker = this.map.getMarker(viewId);

			if (marker.isPseudoMarker)
			{
				marker = this.hybridMarkerBeingEdited;
				viewId = marker.viewId;
			}

			let view = this.map.page.getView(viewId);
			let message = '';

			// Leave a space after the non-breaking spaces to allow text to wrap.
			let gap = '&nbsp;&nbsp; ';

			// Show the marker's hotspot title, but not its Id because it doesn't seem useful and clutters the message.
			let label = viewId === this.primarySelectedMarkerViewId ? 'Selected hotspot' : 'Hotspot';
			message += this.showShortName(label, `${view.title} (${view.hotspotId})`);

			if (this.isMapEditor)
			{
				// Show the marker's name.
				let markerName = this.getMarkerNameFromMarkerData(marker);
				message += `${gap}${this.showShortName("Marker", markerName)}`;

				// Show the marker's style.
				if (marker.styleId !== 0)
				{
					let data = markerStyleData.filter(obj => { return obj.id === marker.styleId });
					message += `${gap}${this.showShortName("Style", data[0].name)}`;
				}

				// Show the marker's coordinates. For zoomable maps, coordinates are at the map scale so that they will
				// always be the same for a marker at any zoom level. For non-zoomable maps, they are screen coordinates.
				let x = marker.x;
				let y = marker.y;
				message += `<br>Location: ${x},${y}`;
			}

			// Show the marker's rotation.
			message += `${gap}Rotation: ${marker.rotationDegrees}&deg;`;

			// Show the marker's attibutes.
			if (this.isMapEditor)
			{
				message += `${gap}Attributes: `;
				if (!marker.isAnchored)
					message += `Not anchored${gap}`;
				if (marker.isHidden)
					message += `Hidden${gap}`;
				if (marker.isDisabled)
					message += `Disabled${gap}`;
				if (!marker.markerZooms)
					message += `Non-zoomable${gap}`;
				if (marker.isStatic)
					message += `Static${gap}`;
				message += marker.isLocked ? `Locked` : `Unlocked`;
			}

			message += `${gap}${this.zoomPanInformation}`;

			// Show the marker's view Id.
			if (this.map.tour.flagShowMarkerViewId)
				message += `${gap}${viewId}`;

			this.showMessage(message);
		}
		else 
		{
			if (this.selectedMarkers.count === 0)
			{
				if (!this.showingNotificationMessage)
					this.showZoomPanInformation();
			}
			else
			{
				this.showMarkerCoordsMessage(this.map.getMarker(this.primarySelectedMarkerViewId));
			}
		}
	}

	showMessage(message, notification = false)
	{
		if (this.ignoreMessage)
			return;

		this.showingNotificationMessage = notification;
		this.setMessageAreaStyle();

		// Display a space for a blank message so that the message area height does not collapse to zero.
		if (message.length === 0)
			message = '&nbsp;';
		this.lastMessage = message;

		this.mapControlsMessageArea.innerHTML = message;
	}

	showShortName(label, name)
	{
		const max = 45;
		let shortName = name;
		if (name.length > max)
			shortName = name.substr(0, max).trim() + "...";
		let html = `${label}: <span class="map-editor-status-highlight">${shortName}</span>`
		return html;
	}

	showToolMessage(actionId, message)
	{
		if (this.ignoreMessage)
			return;

		this.showingNotificationMessage = false;
		this.setMessageAreaStyle();

		message = this.dynamicToolMessage(actionId, message);

		if (!message)
			message = this.lastMessage;

		this.mapControlsMessageArea.innerHTML = message;
	}

	showZoomPanInformation()
	{
		this.showMessage(this.zoomPanInformation);
	}

	stopDragging()
	{
		let dragging = this.draggingMarker || this.draggingHandle || this.draggingCrosshairs;
		if (!dragging && !this.map.panningMap)
			return;

		//console.log(`MapEditor::stopDragging`);

		// Tell the click handling logic to ignore the click event that occurs when marker dragging
		// stops. This is necessary in the situation where the user has selected one or more handles
		// and then ctrl-drags the marker to move it (you can't drag a marker being edited unless
		// the ctrl key is down). Without this flag, the click handler would treat the ctrl-click as
		// a request to deselect all the handles such that when dragging stops, no handles are selected.
		if (this.draggingMarker && this.editingMarker)
			this.ignoreClick = true;

		this.viewIdForMarkerBeingDragged = 0;
		this.draggingMarker = false;
		this.draggingCrosshairs = false;
		this.map.stopPanning();

		if (this.draggingHandle)
		{
			this.draggingHandle = false;
			let marker = this.selectedMarkers.primaryMarker;
			this.addEditToHistory(new MarkerEdit(this.markerBeingEdited));
			marker.handles.handleBeingDragged = null;
		}

		this.turnOffSoftControlKey();
	}

	turnOffSoftControlKey()
	{
		if (this.isGalleryEditor)
			return;

		this.toggleSoftControlKeyEnabled = true;
		this.performAction(this.ACTION_TOGGLE_SOFT_CONTROL_KEY);
	}

	updateControls()
	{
		//console.log(`MapEditor::updateControls`);
		this.selectedMarkers.dumpMarkers(); // FOR DEBUGGING ONLY

		let primaryMarker = this.selectedMarkers.count === 1 ? this.selectedMarkers.primaryMarker : null;

		const isHybridMarker = primaryMarker !== null && primaryMarker.shapeIsHybrid;

		const oneMarkerSelected = this.selectedMarkers.count === 1;
		const oneOrMoreMarkersSelected = this.selectedMarkers.count > 0;
		const twoOrMoreMarkersSelected = this.selectedMarkers.count >= 2;

		for (let control of this.controls)
		{
			let enable = true;
			control.hide(false);

			switch (control.actionId)
			{
				case this.ACTION_TOGGLE_SOFT_CONTROL_KEY:
					enable = true;
					break;

				case this.ACTION_SELECT_ALL_MARKERS:
					enable = !this.editingMarker;
					break;

				case this.ACTION_CHANGE_STACK_ORDER:
					enable =
						oneMarkerSelected &&
						!this.editingMarker &&
						!this.hybridEditingEnabled
					break;

				case this.ACTION_ALLOW_OVER_ZOOM:
					enable = true;
					break;

				case this.ACTION_ROTATE_LEFT:
				case this.ACTION_ROTATE_RIGHT:
					enable =
						oneOrMoreMarkersSelected &&
						!this.editingMarker &&
						!this.hybridEditingEnabled;
					control.hide(this.editingMarker);
					break;

				case this.ACTION_NUDGE_LEFT:
				case this.ACTION_NUDGE_UP:
				case this.ACTION_NUDGE_RIGHT:
				case this.ACTION_NUDGE_DOWN:
					enable =
						this.okToNudgeHandle ||
						oneOrMoreMarkersSelected;
					break;

				case this.ACTION_ALIGN_COLUMN:
				case this.ACTION_ALIGN_ROW:
					enable =
						twoOrMoreMarkersSelected ||
						(this.editingMarker && this.markerBeingEdited.handles.selectedHandleCount >= 2);
					control.hide(!enable);
					break;

				case this.ACTION_TOGGLE_APPEARANCE:
					enable =  true
					break;

				case this.ACTION_TOGGLE_TRANSPARENCY:
					enable = oneOrMoreMarkersSelected
					break;

				case this.ACTION_TOGGLE_LOCK_MARKER:
					enable =
						oneOrMoreMarkersSelected &&
						!this.hybridEditingEnabled;
					break;

				case this.ACTION_GO_TO_EDIT_SCREEN:
					enable =
						oneMarkerSelected &&
						!this.editingMarker &&
						!this.hybridEditingEnabled;
					break;

				case this.ACTION_REMOVE:
					if (this.editingMarker)
						enable = this.okToRemoveHandle(this.markerBeingEdited);
					else
						enable = oneOrMoreMarkersSelected;
					break;

				case this.ACTION_SET_MAP_FOCUS:
					enable =
						this.map.currentMapScale <= 1.0 &&
						(this.map.tour.v4 || this.map.zoomedAllTheWayIn || this.map.zoomedAllTheWayOut) &&
						!this.editingMarker &&
						!this.hybridEditingEnabled;
					control.hide(this.editingMarker);
					break;

				case this.ACTION_SHOW_MAP_FOCUS:
					enable =
						this.map.currentMapScale <= 1.0 &&
						!this.editingMarker &&
						!this.hybridEditingEnabled;
					control.hide(this.editingMarker);
					break;

				case this.ACTION_REPLACE_MARKER:
					enable =
						oneOrMoreMarkersSelected &&
						!this.editingMarker &&
						!this.hybridEditingEnabled;
					break;

				case this.ACTION_REPLACE_MARKER_STYLE:
					enable =
						this.numberOfShapeMarkersSelected > 0 &&
						!this.editingMarker &&
						!this.hybridEditingEnabled;
					break;

				case this.ACTION_ADD_NEW_HOTSPOT:
					enable =
						!this.editingMarker &&
						!this.hybridEditingEnabled
					break;

				case this.ACTION_CREATE_NEW_MARKER:
					enable = !this.editingMarker;
					break;

				case this.ACTION_DUPLICATE_MARKER:
					enable =
						this.oneShapeMarkerSelected &&
						!this.editingMarker &&
						!this.hybridEditingEnabled;
					break;

				case this.ACTION_EDIT_MARKER:
					enable =
						this.oneShapeMarkerSelected &&
						!this.editingMarker;
					break;

				case this.ACTION_CONVERT_HYBRID:
					enable =
						this.hybridEditingEnabled ||
						(this.oneShapeMarkerSelected && !this.editingMarker);
					this.setMarkerHybridImage(isHybridMarker, enable);
					break;

				case this.ACTION_FINISH_EDITING:
					enable = this.editingMarker;
					control.hide(!enable);
					break;

				case this.ACTION_UNDO_EDIT:
					enable = this.allowUndo;
					control.hide(!this.editingMarker);
					break;

				case this.ACTION_REDO_EDIT:
					enable = this.allowRedo;
					control.hide(!this.editingMarker);
					break;

				case this.ACTION_CANCEL_EDITING:
					enable = this.editingMarker;
					control.hide(!enable);
					break;

				default:
					console.log(`>>> UNEXPECTED CONTROL: ${control.actionId}`);
					break;
			}

			control.enable(enable);
		}

		// Update the message.
		this.showMarkerCoordsMessage(primaryMarker);
	}

	updateMarkerAndMapAfterEdit(marker)
	{
		//console.log(`MapEditor::updateMarkerAndMapAfterEdit ${marker.x},${marker.y}`);

		// Record the edit and refresh the handles and bounds for the marker being edited.
		// There will be no marker being edited when this method is called immediately after
		// the user has chosen to exit hybrid marker editing mode.
		if (this.markerBeingEdited)
		{
			if (!this.draggingHandle)
				this.addEditToHistory(new MarkerEdit(this.markerBeingEdited));

			this.markerBeingEdited.handles.invalidateHandles();
			marker.updateBounds();
		}

		this.reportMarkerEdit(marker);
		this.updateControls();

		// Draw the unselected markers.
		if (this.map.markerLayerCacheIsStale)
			this.drawAndCacheUnselectedMarkers();
		else
			this.map.drawCachedMarkerLayer();

		// Draw the edited marker again so that it will appear above any markers that overlap it.
		this.map.drawMarkerOnly(marker, true);

		// When editing a pseudo marker, update the hybrid marker.
		if (marker.isPseudoMarker)
		{
			this.map.createHybridMarkerFromPseudoMarkers(this.hybridMarkerBeingEdited);
			this.reportMarkerEdit(this.hybridMarkerBeingEdited);
		}

		this.eraseEditLayer();
		this.drawOutlineForMarker(marker);

		maChangeDetected();
	}

	updateMarkerPosition(marker, x, y)
	{
		//console.log(`MapEditor::updateMarkerPosition ${marker.x}->${x},${marker.y}->${y} : ${marker.viewId}`);

		// This method sets a marker's position values, but does not reposition the marker on the map.

		// The x,y coordinates are the marker's center on the full size map.
		marker.x = x;
		marker.y = y;

		// This method does floating point calculations. Because of how floating point math works, the
		// precision of the pct values computed server-side by the Tour Builder using C# can differ in
		// the last few decimal places from the values computed here. For example a server-side value
		// could be 0.524580335731415, but the client-side value could be 0.5245803357314148. Ignore
		// this insignificant difference by only using the first 12 digits after the decimal point.

		// Get the old position from the marker.
		let oldPctX = Number(marker.pctX.toFixed(12));
		let oldPctY = Number(marker.pctY.toFixed(12));

		// Calculate the new position. If the marker is anchored, the position is relative to the
		// unscaled map. If the marker is not anchored, the position is relative to the canvas.
		let w = marker.isAnchored ? this.map.mapW_actual : this.map.canvasW;
		let h = marker.isAnchored ? this.map.mapH_actual : this.map.canvasH;
		let newPctX = Number((x / w).toFixed(12));
		let newPctY = Number((y / h).toFixed(12));

		// If the position has not changed, do nothing.
		if (newPctX === oldPctX && newPctY === oldPctY)
			return false;

		// Update the marker's position values.
		marker.pctX = newPctX;
		marker.pctY = newPctY;

		//console.log(`MapEditor::updateMarkerPosition ${marker.viewId} : ${marker.x},${marker.y} : ${marker.pctX},${marker.pctY}`);

		marker.updateBounds();

		return true;
	};

	get zoomPanInformation()
	{
		return `Zoom: ${Math.round(this.map.currentMapScale * 100)}%&nbsp;&nbsp;Pan: ${Math.abs(this.map.panX_map)},${Math.abs(this.map.panY_map)}`;
	}

}

class MapControl
{
	constructor(actionName, actionId)
	{
		this.actionName = actionName;
		this.actionId = actionId;
		this.img = document.getElementById(`mapControl${actionName}`);
		this.classList = this.img.classList;
		this.clickHandler = function () { window.MapEditor.performAction(actionId); };
		let altText = this.img.alt;
		this.img.addEventListener("mouseover", function () { window.MapEditor.showToolMessage(actionId, altText); }, false);
		this.img.addEventListener("mouseout", function () { window.MapEditor.showToolMessage(); }, false);
		this.hide(true);
	}

	activate(activate)
	{
		if (activate)
			this.classList.add("mapControlActivated");
		else
			this.classList.remove("mapControlActivated");
	}

	enable(enable)
	{
		if (enable)
		{
			this.classList.add("mapControlEnabled");
			this.classList.remove("mapControlDisabled");
			this.img.onclick = this.clickHandler;
		}
		else
		{
			this.classList.add("mapControlDisabled");
			this.classList.remove("mapControlEnabled");
			this.img.onclick = null;
		}
	}

	hide(hide)
	{
		if (hide)
			this.classList.add("mapControlHidden");
		else
			this.classList.remove("mapControlHidden");
	}

	setIcon(src)
	{
		return this.img.src = src;
	}
}

class MarkerEdit
{
	constructor(marker)
	{
		this.markerId = marker.markerId;
		this.shapeType = marker.shapeType;
		this.shapeW = marker.shapeW;
		this.shapeH = marker.shapeH;
		this.shapeCoords = marker.shapeCoords;
		this.x = marker.x;
		this.y = marker.y;
		this.pctX = marker.pctX;
		this.pctY = marker.pctY;
 	}

	get data()
	{
		return `${this.markerId};${this.shapeType};${this.shapeW};${this.shapeH};${this.shapeCoords}`;
	}
}

class SelectedMarkers
{
	constructor(mapEditor)
	{
		this.mapEditor = mapEditor;
		this.viewIds = [];
	}

	addMarker(viewId)
	{
		//console.log(`MapEditor::SelectedMarkers::addMarker ${viewId}`);

		// If the marker is already selected, remove it, otherwise add it.
		if (this.contains(viewId))
			this.removeMarker(viewId);
		else
			this.viewIds.push(viewId);

		this.dumpMarkers();
	}

	get count()
	{
		return this.viewIds.length;
	}

	contains(viewId)
	{
		return this.viewIds.indexOf(viewId) >= 0;
	}

	dumpMarkers()
	{
		//let message = 'SELECTED MARKERS: ';
		//for (const viewId of this.viewIds)
		//	message += viewId + ' ';
		//console.log(message);
	}

	get primaryMarker()
	{
		let viewId = this.primaryViewId;
		if (viewId === 0)
			return null;
		return this.mapEditor.map.getMarker(viewId);
	}

	get primaryViewId()
	{
		// The primary view Id is the first one added.
		let length = this.viewIds.length;
		return length ? this.viewIds[0] : 0;
	}

	setPrimaryViewId(viewId)
	{
		this.removeMarker(viewId);
		this.addMarker(viewId);
	}

	removeAllMarkers()
	{
		this.viewIds = [];
	}

	removeMarker(viewId)
	{
		//console.log(`SelectedMarkers::removeMarker ${viewId}`);
		let pos = this.viewIds.indexOf(viewId);
		this.viewIds.splice(pos, 1);
		this.dumpMarkers();
	}
}
