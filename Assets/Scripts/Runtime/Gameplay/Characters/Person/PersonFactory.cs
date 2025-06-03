using System.Threading;
using System.Threading.Tasks;
using Azzazelloqq.DetectionService.Source;
using LightDI.Runtime;
using ResourceLoader;
using Runtime.Gameplay.Characters.Person.Base;
using Scripts.Generated.Addressables;
using TickHandler;
using UnityEngine;

namespace Runtime.Gameplay.Characters.Person
{
public class PersonFactory
{
	private readonly IResourceLoader _resourceLoader;
	private readonly PersonDetectionContext _detectionContext;
	private PersonView _viewPrefab;

	public PersonFactory(
		[Inject] IResourceLoader resourceLoader,
		PersonDetectionContext detectionContext)
	{
		_resourceLoader = resourceLoader;
		_detectionContext = detectionContext;
	}

	public async Task<PersonPresenterBase> CreatePersonAsync(Transform parent, Vector3 spawnPosition, CancellationToken token)
	{
		if (_viewPrefab == null)
		{
			var viewResourceId = ResourceIdsContainer.Characters.СitizenView;
			_viewPrefab = await _resourceLoader.LoadResourceAsync<PersonView>(viewResourceId, token);
		}

		var personView = Object.Instantiate(_viewPrefab, parent);
		var personModel = new PersonModel(3f, _detectionContext);
		
		// Create presenter
		var personPresenter = PersonPresenterFactory.CreatePersonPresenter(
			personView, 
			personModel, 
			_detectionContext);
		
		// Initialize
		await personPresenter.InitializeAsync(token);
		personPresenter.InitializePosition(spawnPosition);
		
		return personPresenter;
	}
}
}
