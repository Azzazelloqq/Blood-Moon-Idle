using System.Threading;
using System.Threading.Tasks;
using LightDI.Runtime;
using ResourceLoader;
using Runtime.Gameplay.Characters.Person.Base;
using Scripts.Generated.Addressables;
using UnityEngine;

namespace Runtime.Gameplay.Characters.Person
{
public class CitizenFactory
{
	private readonly IResourceLoader _resourceLoader;
	private readonly PersonDetectionContext _detectionContext;
	private CitizenView _viewPrefab;

	public CitizenFactory(
		[Inject] IResourceLoader resourceLoader,
		PersonDetectionContext detectionContext)
	{
		_resourceLoader = resourceLoader;
		_detectionContext = detectionContext;
	}

	public async Task<CitizenPresenterBase> CreatePersonAsync(
		Transform parent,
		Vector3 spawnPosition,
		CancellationToken token)
	{
		if (_viewPrefab == null)
		{
			var viewResourceId = ResourceIdsContainer.Characters.СitizenView;
			_viewPrefab = await _resourceLoader.LoadResourceAsync<CitizenView>(viewResourceId, token);
		}

		var personView = Object.Instantiate(_viewPrefab, parent);
		var personModel = new CitizenModel(3f, _detectionContext);

		// Create presenter using auto-generated factory
		var personPresenter = CitizenPresenterFactory.CreateCitizenPresenter(
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