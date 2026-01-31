// Copyright (C) 2009-2022 AvantLogic Corporation: https://www.mapsalive.com

export { MapsAliveMedia };

class MapsAliveMedia
{
	constructor(tour)
	{
		this.tour = tour;
		this.audio = null;
		this.url = "";
	}

	getAudio()
	{
		return this.audio;
	}

	pauseAudio()
	{
		if (this.audio)
			this.audio.pause();
	}

	playAudio(url, toggle = true)
	{
		// This method plays the audio from the beginning whether or not it is already playing.
		// When the toggle is set and the audio for the url is playing, it treats the call as stop audio.

		if (toggle && this.audio && url === this.url && !this.audio.ended)
		{
			this.stopAudio();
			return false;
		}

		// The execution sequence below has been tailored to work on Safari on iOS as well as other
		// browsers. Specifically it first sets the listener for the canplaythorugh event and then
		// assigns the url and calls audio.load. Earlier versions of this code passed the url to the
		// Audio constructor and did not call audio.load. Those versions worked on everything except
		// Safari, but on Safari, the canplaythrough event did not fire. The canplaythrough event is
		// fired when the user agent can play the media, and estimates that enough data has been loaded
		// to play the media to its end without having to stop for further content buffering.
		this.pauseAudio();
		this.url = url;
		this.audio = new Audio();
		this.audio.addEventListener("canplaythrough", event =>
		{
			this.audio.play().catch(function (error)
			{
				return false;
			});
		});
		this.audio.src = url;
		this.audio.load();

		return true;
	}

	playOrResumeAudio(url, toggle = true)
	{
		if (toggle && this.audio && url === this.url && !this.audio.paused)
		{
			this.pauseAudio();
			return false;
		}

		// This method plays new audio from the beginning or resumes the audio if it is already playing.
		if (this.audio && url === this.url)
		{
			this.audio.play().catch(function (error)
			{
				console.log(`Handled exception in MapsAliveMedia::playOrResumeAudio(resume): ${error.message}`);
				return false;
			});
		}
		else
		{
			// This code below is a duplicate of the code from playAudio() above. It's duplicated to avoids the
			// error: "DOMException: play() failed because the user didn't interact with the document first"
			// which occurs if the reqest to play audio is not the result of direct user interaction. In other
			// words, the code can't be factored into a function used by both this method and playAudio because
			// the function would be called directly. Apparently there are other ways to work around this issue
			// but this one is fine since there's not a lot of code involved.
			this.pauseAudio();
			this.url = url;
			this.audio = new Audio();
			this.audio.addEventListener("canplaythrough", event =>
			{
				this.audio.play().catch(function (error)
				{
					return false;
				});
			});
			this.audio.src = url;
			this.audio.load();
		}

		return true;
	}

	stopAudio()
	{
		this.pauseAudio();
		this.audio = null;
	}
}