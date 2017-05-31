package nz.geek.nathan.minesweeper.app

import android.app.Application
import android.content.Context

/**
 * Created by nate on 17/05/17.
 */
class MSApplication: Application() {
    companion object{
        fun get(ctx: Context) = ctx.applicationContext as MSApplication
    }

    val mApplicationComponent: ApplicationComponent by lazy {
            DaggerApplicationComponent.builder()
                    .applicationModule(ApplicationModule(this))
                    .build()
        }
}