#pragma once
#include <memory>

#include "Core/SHObject.h"
#include "Entity.h"
#include "EntityManager.h"
#include "ShadowMath/Transform.h"

class Camera;

namespace ShadowEngine::EntitySystem {

	/// <summary>
	/// Represents a scene, that is composed of Entities.
	/// 
	/// </summary>
	class Scene : public SHObject, public std::enable_shared_from_this<Scene>
	{
		SHObject_Base(Scene)

	public:

		/// <summary>
		/// Represents the Entities in this Scene
		/// </summary>
		/// This is a list of all the top level Entities that are in this Scene
		/// To add to the list use <see cref="AddEntity"/>
		std::list<rtm_ptr<Entity>> m_entities;

		ShadowEntity::Transform centerTransform;

		//Main Camera ref
		Camera* mainCamera;

		Scene() : mainCamera(nullptr) {

		}
		virtual ~Scene() = default;


		virtual void Init();

		
		virtual void Start();
		virtual void Update();

		template<class T, class ...ARGS>
		rtm_ptr<T> AddEntity(ARGS&&... args) {
			rtm_ptr<T> ptr = EntityManager::Instance->AddEntity<T>(std::forward<ARGS>(args)...);
			ptr->scene = this;
			ptr->Build();
			m_entities.push_back(ptr);

			return ptr;
		}

		template<class T>
		void DestroyEntity(rtm_ptr<T>& entity) {
			EntityManager::Instance->RemoveEntity<T>(entity);
			m_entities.remove(entity);
		}

		void DestroyScene() {
			for each (auto var in m_entities)
			{
				EntityManager::Instance->RemoveEntity(var);
			}
		}

		ShadowEntity::Transform* GetCenter() {
			return &centerTransform;
		}
	};

}
