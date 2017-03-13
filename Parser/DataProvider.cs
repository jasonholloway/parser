using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
        
    public static class Blah
    {

        static async Task<IEnumerable<int>> GetDateRange(DateTime dateFrom, DateTime dateTo) {
            throw new NotImplementedException();
        }



        public class Call<TArg1, TArg2>
        {

        }


        public static IDataProvider CreateProvider()
            => DataProvider.Create(root => {

                root.HasSingleton("Profile", profile => {
                    //...
                });

                root.HasSet("CalendarItems", items => {
                    
                    items.Serves(call => {
                        //what are we going to do - pluck parameters out of filter? Yup. The parameters, too, may be via aliases, which will be hidden.

                        return Task.FromResult(Enumerable.Empty<int>());
                    });
                    

                    items.HasFunction("DateRange", call => {          //but function args need to be predeclared...
                        var dateFrom = call.Args[0].As<DateTime>();    //would throw if unbindable at runtime
                        var dateTo = call.Args[1].As<DateTime>();

                        return GetDateRange(dateFrom, dateTo);      //we shouldn't just be returning materializable data, but some abstract schema of the entity at this level
                    });

                    items.HasSingleton("FavouriteOccasion", () => {

                        //here we want to return an entity that itself responds
                        //it will make available functions and properties

                        return Task.FromResult(13);
                    });


                    items.HasSet("Latest", latest => {
                        latest.Serves(call => {
                            return Task.FromResult(Enumerable.Empty<int>());
                        });
                    });
                    
                });


            });


        public static void Go() {
            var provider = CreateProvider();

            //...
        }
    }




    public class CalendarItem
    {

    }




    public static class DataProviderExtensions
    {

        public static void HasSet(this DataProvider dataProv, string name, Action<ISetConfig> config) {
            //...
        }

        public static void HasSingleton(this DataProvider dataProv, string name, Action<ISetConfig> config) {
            //...
        }





        public static void HasFunction<T>(this ISetConfig @set, string name, Func<IFunctionCall, Task<IEnumerable<T>>> resolver)
        {
            throw new NotImplementedException();
        }


        public static void HasSingleton<T>(this ISetConfig @set, string name, Func<Task<T>> resolver) {

        }


        public static void HasSet(this ISetConfig @set, string name, Action<ISetConfig> config) {

        }

        


        public static void Serves<T>(this ISetConfig @set, Func<ICall, Task<IEnumerable<T>>> resolver) {

        }


    }




    public interface ISetConfig<T> : ISetConfig
    {

    }

    public interface ISetConfig
    {

    }

    public interface IItemConfig
    {

    }



    public interface IFunctionCall : ICall
    {
        IIndexer Args { get; }
    }


    public interface ICall
    {
        IFilter Filter { get; }
        IProjection Projection { get; }
        int Top { get; }
        int Skip { get; }
    }


    public interface IFilter
    {
        object Root { get; }
    }

    public interface IProjection
    {
        object Root { get; }
    }


    public interface IIndexer
    {
        IValue this[int i] { get; }
    }


    public interface IValue
    {
        T As<T>();
    }





    public interface IDataProvider
    {

    }



    public class DataProvider : IDataProvider
    {

        private DataProvider() {

        }

        public object Model { get; }


        public static IDataProvider Create(Action<DataProvider> config) 
        {
            var dataProv = new DataProvider();

            config(dataProv);

            return dataProv;
        }

    }
}
