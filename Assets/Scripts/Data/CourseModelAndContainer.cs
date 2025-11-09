using System.Collections.Generic;

namespace MyGame.Models
{
    [System.Serializable]
    public class CourseModel
    {
        public int id;
        public string name;
        public List<int> taskIds = new List<int>();
    }

    [System.Serializable]
    public class CoursesContainer
    {
        public List<CourseModel> courses = new List<CourseModel>();
    }
}
