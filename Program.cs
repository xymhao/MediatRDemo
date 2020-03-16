using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using MediatR;

namespace MediatRDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder
                .RegisterType<Mediator>()
                .As<IMediator>()
                .InstancePerLifetimeScope();
            builder.Register<ServiceFactory>(context =>
            {
                var c = context.Resolve<IComponentContext>();
                return t => c.Resolve(t);
            });
            builder.RegisterAssemblyTypes(typeof(Program).GetTypeInfo().Assembly).AsImplementedInterfaces(); // via assembly scan
            var container = builder.Build();
            var mediator = container.Resolve<IMediator>();

            var response = await mediator.Send(new Ping());
            var response2 = await mediator.Send(new OneWay());
            await mediator.Publish(new PingNotification());

            Console.WriteLine(response); // "Pong"
            Console.WriteLine("Hello World!");
            Console.Read();
        }

        public class Ping : IRequest<string>
        {

        }

        public class PingHandler : IRequestHandler<Ping, string>
        {
            public Task<string> Handle(Ping request, CancellationToken cancellationToken)
            {
                return Task.FromResult("Pong");
            }
        }

        public class PongHandler : IRequestHandler<Ping, string>
        {
            public Task<string> Handle(Ping request, CancellationToken cancellationToken)
            {
                return Task.FromResult("Pong2");
            }
        }

        public class OneWay : IRequest { }

        /// <summary>
        /// 不需要响应
        /// </summary>
        public class OneWayHandlerWithBaseClass : AsyncRequestHandler<OneWay>
        {
            protected override Task Handle(OneWay request, CancellationToken cancellationToken)
            {
                // Twiddle thumbs
                return Task.CompletedTask;

            }
        }

        public class SyncHandler : RequestHandler<Ping, string>
        {
            protected override string Handle(Ping request)
            {
                return "Pong";
            }
        }

        public class PingNotification : INotification { }

        public class Pong1 : INotificationHandler<PingNotification>
        {
            public Task Handle(PingNotification notification, CancellationToken cancellationToken)
            {
                Debug.WriteLine("Pong 1");
                return Task.CompletedTask;
            }
        }

        public class Pong2 : INotificationHandler<PingNotification>
        {
            public Task Handle(PingNotification notification, CancellationToken cancellationToken)
            {
                Debug.WriteLine("Pong 2");
                return Task.CompletedTask;
            }
        }
    }
}
