(ns frontend.localization
  (:require
   [frontend.errors :as e]
   [clojure.string :as str]
   [re-frame.core :as re-frame]))

(defn get-translation [path]
  (let [map @(re-frame/subscribe [:locale/map])
        result (get-in map path)]
    (if (and result (string? result)) result
        (str "translate(" path ")"))))

(defn split-kw [kw]
  (if-let [ns (namespace kw)]
    (conj (mapv keyword (str/split ns ".")) (keyword (name kw)))
    [kw]))

(defn tr [kw]
  (get-translation (split-kw kw)))

(defn add-prefix
  ([kw]
   (add-prefix (fn [] []) kw))
  ([existing kws]
   (let [path (concat (existing) (if (keyword? kws) (split-kw kws) kws))]
     (fn ([] path)
       ([last]
        (get-translation (concat path (split-kw last))))))))

(defn tr-error [str]
  (get-in @(re-frame/subscribe [:locale/map]) [:error str]))

(def one-big-dictionary
  {:eng
   {:login
    {:select
     {:register-new-account "new account"
      :go-back "return"
      :login-old-account "login"}
     :create-new-account "create new account"
     :login "login"
     :password "password"
     :repeat "repeat password"
     :username "username"}
    :about
    {:title "about this app"
     :body "simple habit tracker was created mostly out of boredom, feel free to use this instance, run your own or modify source"
     :license-pre "this software is published under the "
     :license-post " license"
     :source-at "source code avialable at "
     :source-at-github-clickable "github"
     :close "close"}
    :nav
    {:logout "logout"
     :about "about"
     :account "account"
     :habits "habits"}
    :habit
    {:tab {:cts "categories"
           :alerts "alerts"
           :completions "history"}
     :add-new "add new"
     :description "add description.."
     :confirm-deletion "really delete this habit?"
     :tabs-select "select view"
     :new-habit "new habit"
     :name "add name.."}
    :completion
    {:date {:picker-buttons
            {:cancel "cancel"
             :confirm "accept"}
            :pick "pick"
            :now "now"
            :unchanged "without change"
            :today "today"
            :yesterday "yesterday"}
     :editor {:type "category"
              :note "note"
              :color "color"
              :use-color "override color?"
              :cancel "cancel"
              :advanced-datepicker
              {:specify-hour "exact time?"
               :cancel "cancel"
               :confirm "confirm"}}
     :add-new "new completion"
     :new-completion "create new completion"
     :edit-completion "edit completion"

     :no-completion-types "no completion types"
     :add-new-confirm "confirm"
     :edit-confirm "save"}
    :ct
    {:delete-popup
     {:title "specify how to handle completions with this category"
      :confirm "delete category"
      :cancel "cancel"
      :handle "how to modify affected completions"
      :delete "delete completions with this category"
      :color {:label "how to modify completions colors"
              :leave "don't change completion colors"
              :conditional "change only when completion has no color set"
              :always "change completion color even if it already has one"}
      :note "leave/append note?"}
     :add-new "add new"
     :new "category"}
    :prompt
    {:confirm "confirm"
     :cancel "cancel"}
    :error {::e/duplicate-username {:part1 "username" :part2 "taken, pick another"}
            ::e/invalid-username-or-password "invalid username or password"
            ::e/expired-token "token expired, please reload"
            ::e/habit-not-found "habit not found, deleted?"
            ::e/completion-type-not-found "category not found, deleted?"
            ::e/completion-not-found "completion not found, deleted?"
            ::e/unknown-error "unknown error occured :("}}})
