using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Net;
using System.Net.Mail;

namespace PharmacyManager.Services
{
    public class EmailService
    {
        public async Task SendEmailAsync(string to, string subject, string body, string from = null)
        {
            var email = "olothandomvango@gmail.com";
            var password = "ijsqwtimhdvyupfz";

            var smtpClient = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(email, password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("olothandomvango@gmail.com", "IBhayi Pharmacy"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(to);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }

    public class EmailBody()
    {
        public static string Customer()
        {
            return (
                    $"@"
                );
        }

        public static string PharmacistAccount(string name, string surname, string email, string password)
        {
            return $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset=""UTF-8"">
                        <title>Pharmacist Account</title>
                    </head>
                    <body style=""font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;"">
                        <div style=""max-width: 600px; margin: auto; background-color: #ffffff; padding: 20px;      border-      radius:   8px; box-shadow: 0 0 10px rgba(0,0,0,0.1);"">
                            <h2 style=""color: #2c3e50; text-align: center;"">Welcome, {name} {surname}!</h2>
                            <p style=""font-size: 16px; color: #333;"">
                                Your pharmacist account has been created.
                            </p>
                            <p style=""font-size: 16px; color: #333;"">
                                <strong>Email:</strong> {email}<br/>
                                <strong>Temporary Password:</strong> {password}
                            </p>
                          
                            <p style=""font-size: 14px; color: #777;"">
                                Please change your password after logging in for the first time.
                            </p>
                        </div>
                    </body>
                    </html>
                    ";
        }

        public static string SupplierOrderEmail(string supplierName, string orderNumber, DateTime orderDate, List<(string MedicationName, int Quantity)> items)
        {
            var itemRows = string.Join("", items.Select(i => $@"
                <tr>
                    <td style='padding: 8px; border: 1px solid #ddd;'>{i.MedicationName}</td>
                    <td style='padding: 8px; border: 1px solid #ddd; text-align:center;'>{i.Quantity}</td>
                </tr>"));

            return $@"
                <html>
                <body style='font-family: Arial, sans-serif; color: #333; background-color: #f9f9f9; padding: 20px;'>
                    <div style='max-width: 600px; margin: auto; background: #fff; border-radius: 8px; box-shadow: 0 2px 6px rgba(0,0,0,0.1); padding:       20px;'>
                        <h2 style='color: #2c3e50; text-align:center;'>New Order Notification</h2>
                        <p>Dear <strong>{supplierName}</strong>,</p>
                        <p>Below are the details for your new order from IBhayi Pharmacy.</p>

                        <table style='width:100%; border-collapse: collapse; margin-top: 10px;'>
                            <tr style='background-color: #f2f2f2;'>
                                <th style='padding: 8px; border: 1px solid #ddd; text-align:left;'>Medication</th>
                                <th style='padding: 8px; border: 1px solid #ddd; text-align:center;'>Quantity</th>
                            </tr>
                            {itemRows}
                        </table>

                        <p style='margin-top: 20px;'>Order Number: <strong>{orderNumber}</strong><br/>
                        Order Date: {orderDate:dd MMM yyyy}</p>

                        <p>Thank you for your continued partnership.</p>

                        <p style='margin-top: 30px; font-size: 13px; color: #777;'>Kind regards,<br/>
                        <strong>IBhayi Pharmacy Procurement Team</strong></p>
                    </div>
                </body>
                </html>";
        }

        public static string CustomerReadyEmail(string customerName, string prescriptionNumber)
        {
            return ($@"
                       <html>
                       <body style='font-family: Arial, sans-serif; color: #333; background-color: f9f9f9;        padding:   20px;'>
                           <div style='max-width: 600px; margin: auto; background: #fff; border-radius:8px;   box-  shadow:   0     2px     6px rgba(0,0,0,0.1); padding: 20px;'>
                               <h2 style='color: #2c3e50; text-align:center;'>Your Medication is Ready</h2>

                               <p>Dear <strong>{customerName}</strong>,</p>

                               <p>Your prescription <strong>#{prescriptionNumber}</strong> is now readyfor        collection     at  IBhayi   Pharmacy.</p>

                               <p>Please come to the pharmacy at your earliest convenience to collectyour         medication.</   p>

                               <p style='margin-top: 30px; font-size: 13px; color: #777;'>
                                   Kind regards,<br/>
                                   <strong>IBhayi Pharmacy Team</strong>
                               </p>
                           </div>
                       </body>
                       </html>"
            );
        }
        public static string ManagerOrderEmail(string supplierName, string orderNumber, DateTime orderDate, List<(string MedicationName, int Quantity)> items)
        {
            var itemRows = string.Join("", items.Select(i => $@"
        <tr>
            <td style='padding: 8px; border: 1px solid #ddd;'>{i.MedicationName}</td>
            <td style='padding: 8px; border: 1px solid #ddd; text-align:center;'>{i.Quantity}</td>
        </tr>"));

            return $@"
        <html>
        <body style='font-family: Arial, sans-serif; color: #333; background-color: #f9f9f9; padding: 20px;'>
            <div style='max-width: 600px; margin: auto; background: #fff; border-radius: 8px; box-shadow: 0 2px 6px rgba(0,0,0,0.1); padding: 20px;'>
                <h2 style='color: #2c3e50; text-align:center;'>New Order Placed - IBhayi Pharmacy</h2>
                <p>Dear <strong>{supplierName}</strong>,</p>
                <p>A new order has been placed by the pharmacy manager. Please process this order as soon as possible.</p>

                <table style='width:100%; border-collapse: collapse; margin-top: 10px;'>
                    <tr style='background-color: #f2f2f2;'>
                        <th style='padding: 8px; border: 1px solid #ddd; text-align:left;'>Medication</th>
                        <th style='padding: 8px; border: 1px solid #ddd; text-align:center;'>Quantity</th>
                    </tr>
                    {itemRows}
                </table>

                <p style='margin-top: 20px;'>Order Number: <strong>{orderNumber}</strong><br/>
                Order Date: {orderDate:dd MMM yyyy}</p>

                <p>Please confirm receipt of this order and provide an estimated delivery date.</p>

                <p style='margin-top: 30px; font-size: 13px; color: #777;'>Kind regards,<br/>
                <strong>IBhayi Pharmacy Management</strong></p>
            </div>
        </body>
        </html>";
        }

        public static string OrderCancellationEmail(string supplierName, string orderNumber, DateTime orderDate, List<(string MedicationName, int Quantity)> items, string cancellationReason = "Order cancelled by pharmacy manager")
        {
            var itemRows = string.Join("", items.Select(i => $@"
        <tr>
            <td style='padding: 8px; border: 1px solid #ddd;'>{i.MedicationName}</td>
            <td style='padding: 8px; border: 1px solid #ddd; text-align:center;'>{i.Quantity}</td>
        </tr>"));

            return $@"
        <html>
        <body style='font-family: Arial, sans-serif; color: #333; background-color: #f9f9f9; padding: 20px;'>
            <div style='max-width: 600px; margin: auto; background: #fff; border-radius: 8px; box-shadow: 0 2px 6px rgba(0,0,0,0.1); padding: 20px;'>
                <h2 style='color: #dc3545; text-align:center;'>Order Cancellation Notice</h2>
                <p>Dear <strong>{supplierName}</strong>,</p>
                <p>We regret to inform you that the following order has been cancelled:</p>

                <table style='width:100%; border-collapse: collapse; margin-top: 10px;'>
                    <tr style='background-color: #f8d7da;'>
                        <th style='padding: 8px; border: 1px solid #ddd; text-align:left;'>Medication</th>
                        <th style='padding: 8px; border: 1px solid #ddd; text-align:center;'>Quantity</th>
                    </tr>
                    {itemRows}
                </table>

                <p style='margin-top: 20px;'>Order Number: <strong>{orderNumber}</strong><br/>
                Original Order Date: {orderDate:dd MMM yyyy}<br/>
                Cancellation Reason: {cancellationReason}</p>

                <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin-top: 20px;'>
                    <p style='margin: 0; color: #6c757d; font-size: 14px;'>
                        <strong>Note:</strong> Please disregard any previous communications regarding this order. 
                        No further action is required from your side.
                    </p>
                </div>

                <p style='margin-top: 30px; font-size: 13px; color: #777;'>Kind regards,<br/>
                <strong>IBhayi Pharmacy Management</strong></p>
            </div>
        </body>
        </html>";
        }

    }
}