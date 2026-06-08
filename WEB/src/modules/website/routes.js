export default [
  {
    path: "",
    component: () => import("layouts/websiteLayout.vue"),
    children: [
      { path: "", name: "index", component: () => import("modules/website/pages/index.vue"), meta: { title: "Home" } },
      { path: "demo", name: "demoPage", component: () => import("modules/website/pages/bookADemo.vue"), meta: { title: "Book Your Demo" } },
      { path: "privacy-policy", name: "privacy-policy", component: () => import("modules/website/pages/privacyPolicy.vue"), meta: { title: "Privacy Policy" } },
      { path: "/auth/login", name: "login", component: () => import("modules/auth/pages/login.vue"), meta: { title: "Login" } },
      // contactUs → websiteLayout.vue
      { path: "/contactUs", name: "contactus", component: () => import("modules/website/components/addEditContactUs.vue"), meta: { title: "Contact Us" } }
    ]
  },
  {
    path: "/privacy-policy-mobile", name: "privacy-policy-mobile", component: () => import("modules/website/pages/privacyPolicyMobile.vue"), meta: { title: "Privacy Policy" }
  },
  // /contact-us → layout.vue
  {
    path: "/contact-us",
    component: () => import("layouts/layout.vue"),
    children: [
      { path: "", name: "contactUs", component: () => import("modules/website/pages/contactUsList.vue"), meta: { requiresAuth: true, title: "Contacts" } }
    ]
  }
];
