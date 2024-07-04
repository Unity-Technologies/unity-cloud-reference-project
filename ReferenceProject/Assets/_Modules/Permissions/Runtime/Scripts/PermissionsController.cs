using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.Cloud.Common;
using Unity.Cloud.Identity;
using UnityEngine;

namespace Unity.ReferenceProject.Permissions
{
    public interface IPermissionsController
    {
        string[] Permissions { get; }

        Task SetOrganization(IOrganization organization, CancellationToken cancellationToken = default);
        Task SetProject(ProjectId projectId, CancellationToken cancellationToken = default);
        void Reset();
    }

    public class PermissionsController : IPermissionsController
    {
        string[] m_Permissions;
        string[] m_OrganizationPermissions;

        IOrganization m_Organization;

        public string[] Permissions => m_Permissions;

        public async Task SetOrganization(IOrganization organization, CancellationToken cancellationToken = default)
        {
            Reset();

            if (organization == null)
            {
                return;
            }

            m_Organization = organization;
            var permissions = await organization.ListPermissionsAsync();

            m_OrganizationPermissions = permissions.Select(p => p.ToString()).ToArray();
            m_Permissions = permissions.Select(p => p.ToString()).ToArray();
        }

        public async Task SetProject(ProjectId projectId, CancellationToken cancellationToken = default)
        {
            if (m_Organization == null)
                return;

            if (projectId == ProjectId.None)
            {
                m_Permissions = m_OrganizationPermissions;
                return;
            }

            await foreach (var project in m_Organization.ListProjectsAsync(Range.All, cancellationToken))
            {
                if (project.Descriptor.ProjectId == projectId)
                {
                    var permissions = (await project.ListPermissionsAsync()).Select(p => p.ToString());
                    m_Permissions = m_OrganizationPermissions.Concat(permissions).ToArray();
                }
            }
        }

        public void Reset()
        {
            m_Organization = null;
            m_Permissions = null;
            m_OrganizationPermissions = null;
        }
    }
}
