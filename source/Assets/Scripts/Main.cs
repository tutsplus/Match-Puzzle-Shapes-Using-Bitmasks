using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum PageType
{
	None = -1,
	MainPage = 0,
	BitmaskPuzzleShapesGame,
	Max
};

public class Main : MonoBehaviour 
{
	//static Main instance = null;

	private PageType _currentPageType = PageType.None;
	private AbstractPage _currentPage = null;

	// Use this for initialization
	void Start () 
	{
		//instance = this;

		Go.defaultEaseType = EaseType.Linear;
		Go.duplicatePropertyRule = DuplicatePropertyRuleType.RemoveRunningProperty;

		bool landscape = true;
		bool portrait = false;
		
		bool isIPad = SystemInfo.deviceModel.Contains("iPad");
		bool shouldSupportPortraitUpsideDown = isIPad && portrait; //only support portrait upside-down on iPad
		
		FutileParams fparams = new FutileParams(landscape, landscape, portrait, shouldSupportPortraitUpsideDown);
		
		fparams.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
		
		fparams.AddResolutionLevel(480.0f,	1.0f,	1.0f,	"_Scale1"); //iPhone
		fparams.AddResolutionLevel(960.0f,	2.0f,	2.0f,	"_Scale2"); //iPhone retina
		fparams.AddResolutionLevel(1024.0f,	2.0f,	2.0f,	"_Scale2"); //iPad
		fparams.AddResolutionLevel(1280.0f,	2.0f,	2.0f,	"_Scale2"); //Nexus 7
		fparams.AddResolutionLevel(2048.0f,	4.0f,	4.0f,	"_Scale4"); //iPad Retina
		
		fparams.origin = new Vector2(0.0f,0.0f);
		
		Futile.instance.Init (fparams);

		Futile.atlasManager.LoadAtlas("Atlases/UIFonts");
		Futile.atlasManager.LoadAtlas("Atlases/GameAtlas");

		FPWorld.Create(64.0f);

		FTextParams textParams;
		
		textParams = new FTextParams();
		textParams.lineHeightOffset = -8.0f;
		Futile.atlasManager.LoadFont("Franchise","FranchiseFont"+Futile.resourceSuffix, "Atlases/FranchiseFont"+Futile.resourceSuffix, -2.0f,-5.0f,textParams);
		
		textParams = new FTextParams();
		textParams.kerningOffset = -0.5f;
		textParams.lineHeightOffset = -8.0f;
		Futile.atlasManager.LoadFont("CubanoInnerShadow","Cubano_InnerShadow"+Futile.resourceSuffix, "Atlases/CubanoInnerShadow"+Futile.resourceSuffix, 0.0f,2.0f,textParams);

		GoToPage(PageType.BitmaskPuzzleShapesGame);
	}
	
	public void GoToPage (PageType pageType)
	{
		if(_currentPageType == pageType) return; //we're already on the same page, so don't bother doing anything
		
		AbstractPage pageToCreate = null;
		
		switch (pageType) {
		case PageType.BitmaskPuzzleShapesGame:
			pageToCreate = new BitmaskPuzzleGame();
			break;
		}
		
		if(pageToCreate != null) //destroy the old page and create a new one
		{
			_currentPageType = pageType;	
			
			if(_currentPage != null)
			{
				_currentPage.Destroy();
				Futile.stage.RemoveChild(_currentPage);
			}
			
			_currentPage = pageToCreate;
			Futile.stage.AddChild(_currentPage);
			_currentPage.Start();
		}
		
	}
}
