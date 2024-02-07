using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Cloud.Annotation;
using Unity.Cloud.Annotation.Runtime;
using Unity.Cloud.Common;
using Unity.ReferenceProject.Messaging;
using Unity.ReferenceProject.Permissions;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.Annotation
{
    public struct AnnotationData
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public Vector3 CameraPosition { get; set; }
        public Quaternion CameraRotation { get; set; }
        public Vector3 IndicatorPosition { get; set; }
    }

    public enum AnnotationsPermission
    {
        Read,
        Create,
        Delete,
        Edit,
        CreateComment,
        EditComment,
        DeleteComment
    }

    public interface IAnnotationController
    {
        public void Initialize(DatasetDescriptor datasetDescriptor);
        public void Shutdown();

        public void GetTopic(TopicId topicId, Action<ITopic> callback);
        public void CreateTopic(AnnotationData data);
        public void UpdateTopic(ITopic topic, AnnotationData data);
        public void DeleteTopic(ITopic topic);
        public void CreateComment(ITopic topic, string comment);
        public void UpdateComment(ITopic topic, IComment comment, string newContent);
        public void DeleteComment(ITopic topic, IComment comment);
        public bool CheckPermissions(AnnotationsPermission permission);

        public event Action<IEnumerable<ITopic>> Initialized;
        public event Action<ITopic> TopicCreatedOrUpdated;
        public event Action<TopicId> TopicRemoved;
    }

    public class AnnotationController : IAnnotationController
    {
        IAnnotationRepository m_AnnotationRepository;

        public static readonly int k_TextMaxChar = 512;
        static readonly string k_AnnotationsReadPermission = "cmp.annotations.read";
        static readonly string k_AnnotationsCreatePermission = "cmp.annotations.create";
        static readonly string k_AnnotationsEditPermission = "cmp.annotations.update";
        static readonly string k_AnnotationsDeletePermission = "cmp.annotations.delete";
        static readonly string k_AnnotationsCreateCommentPermission = "cmp.annotations.create_comment";
        static readonly string k_AnnotationsEditCommentPermission = "cmp.annotations.update_comment";
        static readonly string k_AnnotationsDeleteCommentPermission = "cmp.annotations.delete_comment";

        public event Action<IEnumerable<ITopic>> Initialized;
        public event Action<ITopic> TopicCreatedOrUpdated;
        public event Action<TopicId> TopicRemoved;

        IServiceHttpClient m_ServiceHttpClient;
        IServiceHostResolver m_ServiceHostResolver;
        IAppMessaging m_AppMessaging;
        IPermissionsController m_PermissionsController;

        [Inject]
        public void Setup(IServiceHttpClient serviceHttpClient, IServiceHostResolver cloudConfiguration,
            IAppMessaging appMessaging, IPermissionsController permissionsController)
        {
            m_ServiceHttpClient = serviceHttpClient;
            m_ServiceHostResolver = cloudConfiguration;
            m_AppMessaging = appMessaging;
            m_PermissionsController = permissionsController;
        }

        public void Initialize(DatasetDescriptor datasetDescriptor)
        {
            try
            {
                _ = InitializeAsync(datasetDescriptor);
            }
            catch (Exception e)
            {
                m_AppMessaging.ShowException(e);
                throw;
            }
        }

        public void Shutdown()
        {
            if (m_AnnotationRepository != null)
            {
                m_AnnotationRepository.TopicCreated -= OnTopicCreatedOrUpdatedNotification;
                m_AnnotationRepository.TopicUpdated -= OnTopicCreatedOrUpdatedNotification;
                m_AnnotationRepository.TopicRemoved -= OnTopicRemovedNotification;
            }

            m_AnnotationRepository = null;
        }

        public void CreateTopic(AnnotationData data)
        {
            var topicCreation = new TopicCreationBuilder();
            topicCreation.SetTitle(data.Title ?? string.Empty);
            topicCreation.SetDescription(data.Description ?? string.Empty);
            topicCreation.SetWorldCameraTransform(new AnnotationTransform(
                new AnnotationPosition(
                    data.CameraPosition.x,
                    data.CameraPosition.y,
                    data.CameraPosition.z),
                new AnnotationQuaternion(data.CameraRotation.x,
                    data.CameraRotation.y,
                    data.CameraRotation.z,
                    data.CameraRotation.w)));
            topicCreation.SetWorldTransform(new AnnotationTransform(
                new AnnotationPosition(data.IndicatorPosition.x, data.IndicatorPosition.y, data.IndicatorPosition.z),
                new AnnotationQuaternion()));

            CreateTopicAsync(topicCreation.GetTopicCreation()).ConfigureAwait(false);
        }

        async Task CreateTopicAsync(ITopicCreation topicCreation)
        {
            try
            {
                await m_AnnotationRepository.CreateTopicAsync(topicCreation);
            }
            catch (ServiceException e)
            {
                m_AppMessaging.ShowException(e);
                throw;
            }
        }

        public void UpdateTopic(ITopic topic, AnnotationData data)
        {
            var topicUpdateBuilder = new TopicUpdateBuilder(topic);
            topicUpdateBuilder.SetTitle(data.Title);
            topicUpdateBuilder.SetDescription(data.Description);
            topicUpdateBuilder.SetWorldCameraTransform(new AnnotationTransform(
                new AnnotationPosition(
                    data.CameraPosition.x,
                    data.CameraPosition.y,
                    data.CameraPosition.z),
                new AnnotationQuaternion(data.CameraRotation.x,
                    data.CameraRotation.y,
                    data.CameraRotation.z,
                    data.CameraRotation.w)));
            topicUpdateBuilder.SetWorldTransform(new AnnotationTransform(
                new AnnotationPosition(data.IndicatorPosition.x, data.IndicatorPosition.y, data.IndicatorPosition.z),
                new AnnotationQuaternion()));
            UpdateTopicAsync(topicUpdateBuilder.GetTopicUpdate()).ConfigureAwait(false);
        }

        async Task UpdateTopicAsync(ITopicUpdate topicUpdate)
        {
            try
            {
                await m_AnnotationRepository.UpdateTopicAsync(topicUpdate);
            }
            catch (ServiceException e)
            {
                m_AppMessaging.ShowException(e);
                throw;
            }
        }

        public void DeleteTopic(ITopic topic)
        {
            DeleteTopicAsync(topic.Id).ConfigureAwait(false);
        }

        async Task DeleteTopicAsync(TopicId topicId)
        {
            try
            {
                await m_AnnotationRepository.DeleteTopicAsync(topicId);
            }
            catch (ServiceException e)
            {
                m_AppMessaging.ShowException(e);
                throw;
            }
        }

        public void CreateComment(ITopic topic, string comment)
        {
            var commentCreationBuilder = new CommentCreationBuilder();
            commentCreationBuilder.SetText(comment);
            CreateCommentAsync(topic, commentCreationBuilder.GetCommentCreation()).ConfigureAwait(false);
        }

        public bool CheckPermissions(AnnotationsPermission permission)
        {
            switch (permission)
            {
                case AnnotationsPermission.Read:
                    return m_PermissionsController.Permissions.Contains(k_AnnotationsReadPermission);
                case AnnotationsPermission.Create:
                    return m_PermissionsController.Permissions.Contains(k_AnnotationsCreatePermission);
                case AnnotationsPermission.Delete:
                    return m_PermissionsController.Permissions.Contains(k_AnnotationsDeletePermission);
                case AnnotationsPermission.Edit:
                    return m_PermissionsController.Permissions.Contains(k_AnnotationsEditPermission);
                case AnnotationsPermission.CreateComment:
                    return m_PermissionsController.Permissions.Contains(k_AnnotationsCreateCommentPermission);
                case AnnotationsPermission.EditComment:
                    return m_PermissionsController.Permissions.Contains(k_AnnotationsEditCommentPermission);
                case AnnotationsPermission.DeleteComment:
                    return m_PermissionsController.Permissions.Contains(k_AnnotationsDeleteCommentPermission);
                default:
                    return false;
            }
        }

        async Task CreateCommentAsync(ITopic topic, ICommentCreation commentCreation)
        {
            try
            {
                await topic.CreateCommentAsync(commentCreation);
            }
            catch (ServiceException e)
            {
                m_AppMessaging.ShowException(e);
                throw;
            }
        }

        public void UpdateComment(ITopic topic, IComment comment, string newContent)
        {
            var commentUpdateBuilder = new CommentUpdateBuilder(comment);
            commentUpdateBuilder.SetText(newContent);
            UpdateCommentAsync(topic, commentUpdateBuilder.GetCommentUpdate()).ConfigureAwait(false);
        }

        async Task UpdateCommentAsync(ITopic topic, ICommentUpdate commentUpdate)
        {
            try
            {
                await topic.UpdateCommentAsync(commentUpdate);
            }
            catch (ServiceException e)
            {
                m_AppMessaging.ShowException(e);
                throw;
            }
        }

        public void DeleteComment(ITopic topic, IComment comment)
        {
            DeleteCommentAsync(topic, comment.Id).ConfigureAwait(false);
        }

        async Task DeleteCommentAsync(ITopic topic, CommentId commentId)
        {
            try
            {
                await topic.DeleteCommentAsync(commentId);
            }
            catch (ServiceException e)
            {
                m_AppMessaging.ShowException(e);
                throw;
            }
        }

        async Task InitializeAsync(DatasetDescriptor datasetDescriptor)
        {
            m_AnnotationRepository = AnnotationRepositoryFactory.Create(datasetDescriptor, m_ServiceHttpClient, m_ServiceHostResolver);

            try
            {
                m_AnnotationRepository.TopicCreated += OnTopicCreatedOrUpdatedNotification;
                m_AnnotationRepository.TopicUpdated += OnTopicCreatedOrUpdatedNotification;
                m_AnnotationRepository.TopicRemoved += OnTopicRemovedNotification;

                var topics = await m_AnnotationRepository.GetTopicsAsync();
                if (topics != null)
                {
                    Initialized?.Invoke(topics);
                }
            }
            catch (ServiceException e)
            {
                m_AppMessaging.ShowException(e);
                throw;
            }
        }

        public void GetTopic(TopicId topicId, Action<ITopic> callback)
        {
            GetTopicAsync(topicId, callback).ConfigureAwait(false);
        }

        async Task GetTopicAsync(TopicId topicId, Action<ITopic> callback)
        {
            try
            {
                var topic = await m_AnnotationRepository.GetTopicAsync(topicId);
                callback?.Invoke(topic);
            }
            catch (ServiceException e)
            {
                m_AppMessaging.ShowException(e);
                throw;
            }
        }

        void OnTopicCreatedOrUpdatedNotification(ITopic topic)
        {
            TopicCreatedOrUpdated?.Invoke(topic);
        }

        void OnTopicRemovedNotification(ITopic topic)
        {
            TopicRemoved?.Invoke(topic.Id);
        }
    }
}
