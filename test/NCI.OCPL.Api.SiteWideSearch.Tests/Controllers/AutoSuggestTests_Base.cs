using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Moq;

namespace NCI.OCPL.Api.SiteWideSearch.Tests.AutoSuggestControllerTests
{
    public class AutosuggestTests_Base
    {
        protected IOptions<AutosuggestIndexOptions> GetMockedAutosuggestIndexOptions()
        {
            Moq.Mock<IOptions<AutosuggestIndexOptions>> config = new Mock<IOptions<AutosuggestIndexOptions>>();
            config
                .SetupGet(o => o.Value)
                .Returns(new AutosuggestIndexOptions()
                {
                    AliasName = "cgov"
                });

            return config.Object;
        }
    }
}
