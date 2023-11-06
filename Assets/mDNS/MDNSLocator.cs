
//using Mono.Zeroconf;
using UnityEngine;

public class MDNSLocator : MonoBehaviour
{ }
//    private ServiceBrowser browser;

//    private void Start()
//    {
//        browser = new ServiceBrowser();
//        browser.ServiceAdded += OnServiceAdded;
//        browser.Browse("_http._tcp", "local"); // Adjust the service type if different
//    }

//    private void OnServiceAdded(object sender, ServiceBrowseEventArgs args)
//    {
//        // Check if the service name is "esp32"
//        if (args.Service.Name == "esp32")
//        {
//            args.Service.Resolved += OnServiceResolved;
//            args.Service.Resolve();
//        }
//    }

//    private void OnServiceResolved(object sender, ServiceResolvedEventArgs args)
//    {
//        IResolvableService service = args.Service;
//        Debug.Log($"ESP32 service found: {service.Name}");
//        foreach (var address in service.HostEntry.AddressList)
//        {
//            Debug.Log($"IP Address: {address}");
//        }
//    }

//    private void OnDestroy()
//    {
//        if (browser != null)
//        {
//            browser.Dispose();
//        }
//    }
//}
