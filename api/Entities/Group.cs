using System.ComponentModel.DataAnnotations;

namespace api.Entities
{
    public class Group
    {
        // to satisfy Entity framework when it creates the schema for our database,
        // we have to give this an empty constructor with no parameters
        public Group() {}

        public Group(string name)
        {
            Name = name;
        }

        // we don't give this class an id property,
        // but the name of the group should always be unique in our database,
        // and we'll achieve that specifying that Name property is going
        // to be our primary key with [Key]

        [Key]
        public string Name { get; set; }
        public ICollection<Connection> Connections { get; set; } = new List<Connection>();
    }
}