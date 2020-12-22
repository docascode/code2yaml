#include "shpch.h"
#include "Scene.h"

namespace ShadowEngine::EntitySystem {

	void EntitySystem::Scene::Start()
	{
		for (auto& entity : m_entities)
		{
			//entity->Start();
		}
	}

	void Scene::Update()
	{
		for (auto& entity : m_entities)
		{
			//entity->Update(dt);
		}
	}

	void Scene::Init() {

		for (auto& entity : m_entities)
		{
			//entity->Init();
		}
	}
}