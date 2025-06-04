mergeInto(LibraryManager.library, {

  HISPLAYERUnity_Init: function (url, serverKey) {
    setUpContext(GL);
  },

  HISPLAYERUnity_Resume: function () {
    multiView.resume();
  },

  HISPLAYERUnity_Pause: function () {
    multiView.pause();
  },

  HISPLAYERUnity_Stop : function () {
    multiView.stopPlayer();
  },

  HISPLAYERUnity_Seek: function (milliseconds) {
    multiView.seek(milliseconds);
  },

  HISPLAYERUnity_GetVideoDuration: function () {
    return getVideoDuration();
  },

  HISPLAYERUnity_GetCurrentTime: function () {
    return multiView.getCurrentTime();
  },

  HISPLAYERUnity_GetDuration: function () {
    return multiView.getDuration();
  },

  HISPLAYERUnity_GetAdDuration: function () {
    return multiView.getAdDuration();
  },

  HISPLAYERUnity_GetAdRemainingTime: function () {
    return multiView.getAdRemainingTime();
  },

  HISPLAYERUnity_GetAdCurrentTime: function () {
    return multiView.getAdCurrentTime();
  },

  HISPLAYERUnity_IsLive: function () {
    return multiView.isLive();
  },

  HISPLAYERUnity_ChangeVideoContent: function (url, resumePosition, ads) {
    return multiView.changeVideoContent(UTF8ToString(url), resumePosition, UTF8ToString(ads));
  },

  HISPLAYERUnity_SetTrack: function (idx) {
    multiView.setCurrentTrack(idx);
  },

  HISPLAYERUnity_GetCurrentTrackID: function (index) {
    return multiView.getCurrentTrackID();
  },

  HISPLAYERUnity_GetTrackID: function (index) {
    var idStr = multiView.getTrackID(index);
    var bufferSize = lengthBytesUTF8(idStr)+1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(idStr, buffer, bufferSize);
    return buffer;
  },

  HISPLAYERUnity_GetTrackBitrate: function (index) {
    return multiView.getTrackBitrate(index);
  },

  HISPLAYERUnity_GetTrackWidth: function (index) {
    return multiView.getTrackWidth(index);
  },

  HISPLAYERUnity_GetTrackHeight: function (index) {
    return multiView.getTrackHeight(index);
  },

  HISPLAYERUnity_GetTrackCount: function () {
    return multiView.getTrackCount();
  },

  HISPLAYERUnity_GetPlayerStatus : function() {
    return multiView.getPlayerStatus();
  },

  HISPLAYERUnity_EventQueueIsEmpty: function () {
    return isEventQueueEmpty();
  },

  HISPLAYERUnity_GetEventType: function() {
    return getEventType();
  },

  HISPLAYERUnity_GetEventPlayerIndex: function() {
    return getEventPlayerIndex();
  },
  
  HISPLAYERUnity_GetErrorPlayerIndex: function() {
    return getErrorPlayerIndex();
  },

  HISPLAYERUnity_GetJsonFormattedEvent: function() {
    var returnStr = getEventInfo();
    var bufferSize = lengthBytesUTF8(returnStr) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(returnStr, buffer, bufferSize);
    return buffer;
  },

  HISPLAYERUnity_ErrorQueueIsEmpty: function () {
    return isErrorQueueEmpty();
  },

  HISPLAYERUnity_GetErrorType: function() {
    return getErrorType();
  },

  HISPLAYERUnity_GetJsonFormattedError: function() {
    var returnStr = getErrorInfo();
    var bufferSize = lengthBytesUTF8(returnStr) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(returnStr, buffer, bufferSize);
    return buffer;
  },

  HISPLAYERUnity_IsFirefox : function () {
    return navigator.userAgent.search("Firefox") > -1;
  },

  HISPLAYERUnity_SetMultistream : function () {
    if (window.multiView) {
      multiView.release();
    }
    multiView = new MultipleView();
  },

  HISPLAYERUnity_InitializeMultistream : function (hisPlayerKey, sdkVersion) {
    multiView.Initialize(UTF8ToString(hisPlayerKey), UTF8ToString(sdkVersion));
  },

  HISPLAYERUnity_GetMultistreamContentInfoInt : function (elementIndex, info_index) {
    return multiView.getContentInfo(elementIndex, info_index);
  },

  HISPLAYERUnity_updateMultistreamVideoTexture : function (textureId, elementIndex, textureWidth, textureHeight) {
    return multiView.updateVideoTexture(GL.textures[textureId], elementIndex, GL.currentContext, textureWidth, textureHeight);
  },

  HISPLAYERUnity_SetMultiPaths : function (index, path, autoPlayback, loopPlay, disableABR, manifestTimeout, segmentsTimeout, startingBitrate) {
    multiView.setMultiPaths(index, UTF8ToString(path), Boolean(autoPlayback), Boolean(loopPlay), Boolean(disableABR), manifestTimeout, segmentsTimeout, startingBitrate);
  },

  HISPLAYERUnity_SetMultiAdsProperties : function (
    index,
    adsMode,
    assetKey,
    contentSrcId,
    videoId,
    baseUrl,
    manifestUrl,
    adsParams,
    adTagUri,
    enableVpaid,
    pageUrl) {
    multiView.setMultiAdsProperties(
      index,
      UTF8ToString(adsMode),
      UTF8ToString(assetKey),
      UTF8ToString(contentSrcId),
      UTF8ToString(videoId),
      UTF8ToString(baseUrl),
      UTF8ToString(manifestUrl),
      UTF8ToString(adsParams),
      UTF8ToString(adTagUri),
      Boolean(enableVpaid),
      UTF8ToString(pageUrl));
  },

  HISPLAYERUnity_SetMultistreamVolume : function (value) {
    multiView.setVolume(value);
  },

  HISPLAYERUnity_ChangeControlInstance : function (value) {
    multiView.changeControlInstance(value);
  },

  HISPLAYERUnity_DisableABR : function () {
    multiView.disableABR();
  },

  HISPLAYERUnity_EnableABR : function () {
    multiView.enableABR();
  },

  HISPLAYERUnity_Release : function () {
    multiView.release();
  },

  HISPLAYERUnity_CheckWatermark : function (cb) {
    multiView.checkWaterMark().then(function (watermark) {
      dynCall_vi(cb, watermark);
    });    
  },

  HISPLAYERUnity_CheckLicenseError : function (cb) {
    multiView.checkLicenseError().then(function (errorCode) {
      dynCall_vi(cb, errorCode);
    });    
  },
  
  HISPLAYERUnity_GetPlaybackDurationLimit : function() {
	return getPlaybackDurationLimit();
  },

  HISPLAYERUnity_GetIsDurationLimitReady : function() {
	return getIsDurationLimitReady();
  },

  HISPLAYERUnity_Log : function (level, message) {
    Logger.getInstance().log(level, UTF8ToString(message));
  },

  HISPLAYERUnity_SetLogLevel : function (level) {
    Logger.getInstance().setLogLevel(level);
  },
});
