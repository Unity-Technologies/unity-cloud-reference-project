using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.ReferenceProject.AssetList
{ 
    public class AssetDisplayedInformation : ScriptableObject
    {
        [SerializeField]
        bool m_Status;

        [SerializeField]
        bool m_AssetName;

        [SerializeField]
        bool m_Tags;

        [SerializeField]
        bool m_Id;
        
        [SerializeField]
        bool m_Version;

        [SerializeField]
        bool m_Description;

        [SerializeField]
        bool m_SystemTags;

        [SerializeField]
        bool m_PortalMetadata;

        [SerializeField]
        bool m_PreviewFile;

        [SerializeField]
        bool m_StorageId;
        
        [Header("Authoring Info")]

        [SerializeField]
        bool m_CreatedBy;
        
        [SerializeField]
        bool m_Created;
        
        [SerializeField]
        bool m_UpdateBy;
        
        [SerializeField]
        bool m_Updated;
        
        public bool Status => m_Status;
        public bool Name => m_AssetName;
        public bool Tags => m_Tags;
        public bool Id =>  m_Id;
        public bool AuthoringInfo => m_Created || m_CreatedBy || m_UpdateBy || m_Updated;
        public bool Version => m_Version;
        public bool Description => m_Description;
        public bool SystemTags => m_SystemTags;
        public bool PortalMetadata => m_PortalMetadata;
        public bool PreviewFile => m_PreviewFile;
        public bool StorageId => m_StorageId;
        public bool CreatedBy => m_CreatedBy;
        public bool Created => m_Created;
        public bool UpdatedBy => m_UpdateBy;
        public bool Updated => m_Updated;

        public Dictionary<string, bool> GetAllInformations()
        {
            var dict = new Dictionary<string, bool>
            {
                [nameof(Status)] = m_Status,
                [nameof(Name)] = m_AssetName,
                [nameof(Tags)] = m_Tags,
                [nameof(Id)] = m_Id,
                [nameof(Version)] = m_Version,
                [nameof(Description)] = m_Description,
                [nameof(SystemTags)] = m_SystemTags,
                [nameof(PortalMetadata)] = m_PortalMetadata,
                [nameof(PreviewFile)] = m_PreviewFile,
                [nameof(StorageId)] = m_StorageId,
                [nameof(AuthoringInfo)] = m_Created || m_CreatedBy || m_UpdateBy || m_Updated
            };

            return dict;
        }
        
        public Dictionary<string, bool> GetAuthoringInfo()
        {
            var dict = new Dictionary<string, bool>
            {
                [nameof(CreatedBy)] = m_CreatedBy,
                [nameof(Created)] = m_Created,
                [nameof(UpdatedBy)] = m_UpdateBy,
                [nameof(Updated)] = m_Updated
            };
            return dict;
        }
    }
}
